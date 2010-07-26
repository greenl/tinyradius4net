﻿using System;
using System.Data.SqlClient;
using System.Net;
using TinyRadius.Net.Packet;

namespace TinyRadiusService
{
    internal class RadiusServer : TinyRadius.Net.Util.RadiusServer
    {
        public override string GetSharedSecret(IPEndPoint client)
        {
            return Cfg.Instance.TinyConfig.NasSettings[client.Address.ToString()];
        }

        public override string GetUserPassword(string userName)
        {
            if (Cfg.Instance.TinyConfig.ValidateByDatabase)
            {
                using (var conn = new SqlConnection(Cfg.Instance.TinyConfig.DatabaseSetting.Connection))
                {
                    conn.Open();
                    SqlCommand comm = conn.CreateCommand();
                    comm.CommandText = Cfg.Instance.TinyConfig.DatabaseSetting.PasswordSql;
                    SqlParameter param = comm.CreateParameter();
                    param.ParameterName = "@userName";
                    param.Value = userName;
                    comm.Parameters.Add(param);
                    comm.ExecuteScalar().ToString();
                }
            }
            throw new ApplicationException("Please enable Ldap validation or database validation");
        }

        public override RadiusPacket AccessRequestReceived(AccessRequest accessRequest, IPEndPoint client)
        {
            if (Cfg.Instance.TinyConfig.ValidateByLdap)
            {
                string struser = accessRequest.UserName;
                string strpwd = accessRequest.Password;
                string path = Cfg.Instance.TinyConfig.LdapSetting.Path;

                int type = RadiusPacket.AccessReject;

                var auth = new LdapAuthentication(path);
                if (auth.IsAuthenticated(Cfg.Instance.TinyConfig.LdapSetting.DomainName, struser, strpwd))
                {
                    type = RadiusPacket.AccessAccept;
                }


                if (type == RadiusPacket.AccessAccept)
                {
                    var answer = new RadiusPacket(type, accessRequest.Identifier);
                    CopyProxyState(accessRequest, answer);
                    return answer;
                }
                else
                {
                    return base.AccessRequestReceived(accessRequest, client);
                }
            }
            else
            {
                return base.AccessRequestReceived(accessRequest, client);
            }
        }
    }
}