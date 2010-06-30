/**
 * $Id: VendorSpecificAttribute.java,v 1.7 2005/11/22 10:18:38 wuttke Exp $
 * Created on 10.04.2005
 * @author Matthias Wuttke
 * @version $Revision: 1.7 $
 */
namespace TinyRadius.Net.Attribute
{

    /*using java.io.ByteArrayOutputStream;
    using java.io.IOException;
    using java.util.ArrayList;
    using java.util.Iterator;
    using java.util.LinkedList;
    using System.Collections;*/

    using TinyRadius.Net.Directories;

    using TinyRadius.Net.Util;
    using System;
    using System.Collections;
    using System.Text;

    /**
     * This class represents a "Vendor-Specific" attribute.
     */
    public class VendorSpecificAttribute : RadiusAttribute
    {

        /**
         * Radius attribute type code for Vendor-Specific
         */


        public static readonly int VENDOR_SPECIFIC = 26;

        /**
         * Constructs an empty Vendor-Specific attribute that can be read from a
         * Radius packet.
         */
        public VendorSpecificAttribute()
        {
        }

        /**
         * Constructs a new Vendor-Specific attribute to be sent.
         * @param vendorId vendor ID of the sub-attributes
         */
        public VendorSpecificAttribute(int vendorId)
        {
            setAttributeType(VENDOR_SPECIFIC);
            setChildVendorId(vendorId);
        }

        /**
         * Sets the vendor ID of the child attributes.
         * @param childVendorId
         */
        public void setChildVendorId(int childVendorId)
        {
            this.childVendorId = childVendorId;
        }

        /**
         * Returns the vendor ID of the sub-attributes.
         * @return vendor ID of sub attributes
         */
        public int getChildVendorId()
        {
            return childVendorId;
        }

        /**
         * Also copies the new dictionary to sub-attributes.
         * @param dictionary dictionary to set
         * @see TinyRadius.attribute.RadiusAttribute#setDictionary(TinyRadius.dictionary.Hashtable)
         */
        public void setDictionary(System.Collections.Hashtable dictionary)
        {
            super.setDictionary(dictionary);
            for (Iterator i = subAttributes.iterator(); i.hasNext(); )
            {
                RadiusAttribute attr = (RadiusAttribute)i.next();
                attr.setDictionary(dictionary);
            }
        }

        /**
         * Adds a sub-attribute to this attribute.
         * @param attribute sub-attribute to add
         */
        public void addSubAttribute(RadiusAttribute attribute)
        {
            if (attribute.getVendorId() != getChildVendorId())
                throw new ArgumentException(
                        "sub attribut has incorrect vendor ID");

            subAttributes.add(attribute);
        }

        /**
         * Adds a sub-attribute with the specified name to this attribute.
         * @param name name of the sub-attribute
         * @param value value of the sub-attribute
         * @exception ArgumentException invalid sub-attribute name or value
         */
        public void addSubAttribute(String name, String value)
        {
            if (name == null || name.length() == 0)
                throw new ArgumentException("type name is empty");
            if (value == null || value.length() == 0)
                throw new ArgumentException("value is empty");

            AttributeType type = getDictionary().getAttributeTypeByName(name);
            if (type == null)
                throw new ArgumentException("unknown attribute type '"
                        + name + "'");
            if (type.getVendorId() == -1)
                throw new ArgumentException("attribute type '" + name
                        + "' is not a Vendor-Specific sub-attribute");
            if (type.getVendorId() != getChildVendorId())
                throw new ArgumentException("attribute type '" + name
                        + "' does not belong to vendor ID " + getChildVendorId());

            RadiusAttribute attribute = createRadiusAttribute(getDictionary(),
                    getChildVendorId(), type.getTypeCode());
            attribute.setAttributeValue(value);
            addSubAttribute(attribute);
        }

        /**
         * Removes the specified sub-attribute from this attribute.
         * @param attribute RadiusAttribute to remove
         */
        public void removeSubAttribute(RadiusAttribute attribute)
        {
            if (!subAttributes.remove(attribute))
                throw new ArgumentException("no such attribute");
        }

        /**
         * Returns the list of sub-attributes.
         * @return ArrayList of RadiusAttribute objects
         */
        public ArrayList getSubAttributes()
        {
            return subAttributes;
        }

        /**
         * Returns all sub-attributes of this attribut which have the given type.
         * @param attributeType type of sub-attributes to get
         * @return list of RadiusAttribute objects, does not return null
         */
        public ArrayList getSubAttributes(int attributeType)
        {
            if (attributeType < 1 || attributeType > 255)
                throw new ArgumentException(
                        "sub-attribute type out of bounds");

            var result = new ArrayList();
            for (Iterator i = subAttributes.iterator(); i.hasNext(); )
            {
                RadiusAttribute a = (RadiusAttribute)i.next();
                if (attributeType == a.getAttributeType())
                    result.add(a);
            }
            return result;
        }

        /**
         * Returns a sub-attribute of the given type which may only occur once in
         * this attribute.
         * @param type sub-attribute type
         * @return RadiusAttribute object or null if there is no such sub-attribute
         * @throws RuntimeException if there are multiple occurences of the
         * requested sub-attribute type
         */
        public RadiusAttribute getSubAttribute(int type)
        {
            ArrayList attrs = getSubAttributes(type);
            if (attrs.size() > 1)
                throw new RuntimeException(
                        "multiple sub-attributes of requested type " + type);
            else if (attrs.size() == 0)
                return null;
            else
                return (RadiusAttribute)attrs.get(0);
        }

        /**
         * Returns a single sub-attribute of the given type name.
         * @param type attribute type name
         * @return RadiusAttribute object or null if there is no such attribute
         * @throws RuntimeException if the attribute occurs multiple times
         */
        public RadiusAttribute getSubAttribute(String type)
        {
            if (type == null || type.length() == 0)
                throw new ArgumentException("type name is empty");


            AttributeType t = getDictionary().getAttributeTypeByName(type);
            if (t == null)
                throw new ArgumentException("unknown attribute type name '"
                        + type + "'");
            if (t.getVendorId() != getChildVendorId())
                throw new ArgumentException("vendor ID mismatch");

            return getSubAttribute(t.getTypeCode());
        }

        /**
         * Returns the value of the Radius attribute of the given type or null if
         * there is no such attribute.
         * @param type attribute type name
         * @return value of the attribute as a string or null if there is no such
         * attribute
         * @throws ArgumentException if the type name is unknown
         * @throws RuntimeException attribute occurs multiple times
         */
        public String getSubAttributeValue(String type)
        {
            RadiusAttribute attr = getSubAttribute(type);
            if (attr == null)
                return null;
            else
                return attr.getAttributeValue();
        }

        /**
         * Renders this attribute as a byte array.
         * @see TinyRadius.attribute.RadiusAttribute#writeAttribute()
         */
        public byte[] writeAttribute()
        {
            // write vendor ID
            ByteArrayOutputStream bos = new ByteArrayOutputStream(255);
            bos.write(getChildVendorId() >> 24 & 0x0ff);
            bos.write(getChildVendorId() >> 16 & 0x0ff);
            bos.write(getChildVendorId() >> 8 & 0x0ff);
            bos.write(getChildVendorId() & 0x0ff);

            // write sub-attributes
            try
            {
                for (Iterator i = subAttributes.iterator(); i.hasNext(); )
                {
                    RadiusAttribute a = (RadiusAttribute)i.next();
                    bos.write(a.writeAttribute());
                }
            }
            catch (IOException ioe)
            {
                // occurs never
                throw new RuntimeException("error writing data", ioe);
            }

            // check data length
            byte[] attrData = bos.toByteArray();
            int len = attrData.length;
            if (len > 253)
                throw new RuntimeException("Vendor-Specific attribute too long: "
                        + bos.size());

            // compose attribute
            byte[] attr = new byte[len + 2];
            attr[0] = VENDOR_SPECIFIC; // code
            attr[1] = (byte)(len + 2); // length
            System.arraycopy(attrData, 0, attr, 2, len);
            return attr;
        }

        /**
         * Reads a Vendor-Specific attribute and decodes the internal sub-attribute
         * structure.
         * @see TinyRadius.attribute.RadiusAttribute#readAttribute(byte[], int,
         * int)
         */
        public void readAttribute(byte[] data, int offset, int length)
        {
            // check length
            if (length < 6)
                throw new RadiusException("Vendor-Specific attribute too short: "
                        + length);

            int vsaCode = data[offset];
            int vsaLen = ((int)data[offset + 1] & 0x000000ff) - 6;

            if (vsaCode != VENDOR_SPECIFIC)
                throw new RadiusException("not a Vendor-Specific attribute");

            // read vendor ID and vendor data
            /*
             * int vendorId = (data[offset + 2] << 24 | data[offset + 3] << 16 |
             * data[offset + 4] << 8 | ((int)data[offset + 5] & 0x000000ff));
             */
            int vendorId = (unsignedByteToInt(data[offset + 2]) << 24
                    | unsignedByteToInt(data[offset + 3]) << 16
                    | unsignedByteToInt(data[offset + 4]) << 8 | unsignedByteToInt(data[offset + 5]));
            setChildVendorId(vendorId);

            // validate sub-attribute structure
            int pos = 0;
            int count = 0;
            while (pos < vsaLen)
            {
                if (pos + 1 >= vsaLen)
                    throw new RadiusException("Vendor-Specific attribute malformed");
                // int vsaSubType = data[(offset + 6) + pos] & 0x0ff;
                int vsaSubLen = data[(offset + 6) + pos + 1] & 0x0ff;
                pos += vsaSubLen;
                count++;
            }
            if (pos != vsaLen)
                throw new RadiusException("Vendor-Specific attribute malformed");

            subAttributes = new ArrayList(count);
            pos = 0;
            while (pos < vsaLen)
            {
                int subtype = data[(offset + 6) + pos] & 0x0ff;
                int sublength = data[(offset + 6) + pos + 1] & 0x0ff;
                RadiusAttribute a = createRadiusAttribute(getDictionary(),
                        vendorId, subtype);
                a.readAttribute(data, (offset + 6) + pos, sublength);
                subAttributes.add(a);
                pos += sublength;
            }
        }

        private static int unsignedByteToInt(byte b)
        {
            return (int)b & 0xFF;
        }

        /**
         * Returns a string representation for debugging.
         * @see TinyRadius.attribute.RadiusAttribute#toString()
         */
        public String toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Vendor-Specific: ");
            int vendorId = getChildVendorId();
            String vendorName = getDictionary().getVendorName(vendorId);
            if (vendorName != null)
            {
                sb.Append(vendorName);
                sb.Append(" (");
                sb.Append(vendorId);
                sb.Append(")");
            }
            else
            {
                sb.Append("vendor ID ");
                sb.Append(vendorId);
            }
            for (Iterator i = getSubAttributes().iterator(); i.hasNext(); )
            {
                RadiusAttribute attr = (RadiusAttribute)i.next();
                sb.append("\n");
                sb.append(attr.toString());
            }
            return sb.toString();
        }

        /**
         * Sub attributes. Only set if isRawData == false.
         */
        private ArrayList subAttributes = new ArrayList();

        /**
         * Vendor ID of sub-attributes.
         */
        private int childVendorId;
    }

}