/**
 * $Id: StringAttribute.java,v 1.1.1.1 2005/04/17 14:51:33 wuttke Exp $
 * Created on 08.04.2005
 * @author Matthias Wuttke
 * @version $Revision: 1.1.1.1 $
 */
using System;
namespace TinyRadius.Net.Attribute
{


    /**
     * This class represents a Radius attribute which only
     * contains a string.
     */
    public class StringAttribute : RadiusAttribute
    {

        /**
         * Constructs an empty string attribute.
         */
        public StringAttribute()
        {

        }

        /**
         * Constructs a string attribute with the given value.
         * @param type attribute type
         * @param value attribute value
         */
        public StringAttribute(int type, String value)
        {
            setAttributeType(type);
            setAttributeValue(value);
        }

        /**
         * Returns the string value of this attribute.
         * @return a string
         */
        public String getAttributeValue()
        {
            try
            {
                return new String(getAttributeData(), "UTF-8");
            }
            catch (UnsupportedEncodingException uee)
            {
                return new String(getAttributeData());
            }
        }

        /**
         * Sets the string value of this attribute.
         * @param value string, not null
         */
        public void setAttributeValue(String value)
        {
            if (value == null)
                throw new ArgumentNullException("string value not set");
            try
            {
                setAttributeData(value.getBytes("UTF-8"));
            }
            catch (UnsupportedEncodingException uee)
            {
                setAttributeData(value.getBytes());
            }
        }

    }
}