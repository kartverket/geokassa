using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace gridfiles
{ 
    [Serializable] 
    public class Item : IItem
    {
        /*
           public new enum ItemType
        {
            metre = 0,
            [XmlEnum(Name = "US survey foot")]
            USsurveyfoot = 1,
            degree = 2,
            [XmlEnum(Name = "arc-second")]
            arcsecond = 3,
            [XmlEnum(Name = "millimetres per year")]
            millimetresperyear = 4
        } 
        */

        // TYPE:
        //
        // HORIZONTAL_OFFSET
        //    latitude_offset
        //    longitude_offset
        //    latitude_offset_accuracy
        //    longitude_offset_accuracy
        //
        // VERTICAL_OFFSET_GEOGRAPHIC_TO_VERTICAL
        //    geoid_undulation
        //
        // VERTICAL_OFFSET_VERTICAL_TO_VERTICAL
        //    vertical_offset
        //
        // GEOCENTRIC_TRANSLATION
        //    x_translation 
        //    y_translation 
        //    z_translation
        //
        // VELOCITY
        //    east_velocity 
        //    north_velocity
        //    up_velocity

        // UNITTYPE:
        //    metre
        //    US survey foot
        //    degree
        //    arc-second
        //    millimetres per year

        /*
           public new enum ItemType
           {
        latitude_offset = 0,
        longitude_offset = 1,
        latitude_offset_accuracy = 2,
        longitude_offset_accuracy = 3,
        geoid_undulation = 4,
        vertical_offset = 5,
        x_translation = 6,
        y_translation = 7,
        z_translation = 8,
        east_velocity = 9,
        north_velocity = 10,
        up_velocity = 11
    }
    */
        
        public enum NameType
        {
            UNITTYPE = 0,
            DESCRIPTION = 1,
            OFFSET = 2,
            SCALE = 3,
            positive_value = 4,
            area_of_use = 5,
            TYPE = 6,
            grid_name = 7,
            target_crs_epsg_code = 8,
            source_crs_epsg_code = 9,
            target_crs_wkt = 10,
            source_crs_wkt = 11
        }

        private NameType _name;

        public Item()
        {
        }

        [XmlAttribute("name")]
        public NameType Name
        {
            get => _name;
            set
            {
                if (value == NameType.DESCRIPTION)
                    Role = "description";
                if (value == NameType.UNITTYPE)
                    Role = "unittype";
                if (value == NameType.OFFSET)
                    Role = "offset";
                if (value == NameType.SCALE)
                    Role = "scale";

                _name = value;
            }
        }
        
        [DefaultValue(false)]
        [XmlAttribute("sample")]
        public string Sample { get; set; }

        [DefaultValue(false)]
        [XmlAttribute("role")]
        public string Role { get; set; }

        [XmlText]
        public String MyString { get; set; } = "";

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}
