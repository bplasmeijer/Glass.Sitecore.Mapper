﻿/*
   Copyright 2011 Michael Edwards
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Glass.Sitecore.Mapper.Configuration.Attributes;
using Glass.Sitecore.Mapper.Configuration;
using Sitecore.Data.Items;
using Sitecore.Data;

namespace Glass.Sitecore.Mapper.Data
{
    public class SitecoreFieldClassHandler:AbstractSitecoreField
    {
        public override bool WillHandle(Glass.Sitecore.Mapper.Configuration.SitecoreProperty property, InstanceContext context)
        {
            if (!(property.Attribute is SitecoreFieldAttribute)) return false;
            return context.Classes.ContainsKey(property.Property.PropertyType);
        }



        public override object GetFieldValue(string fieldValue, object parent, Item item, SitecoreProperty property, InstanceContext context)
        {
            Item target = null;

            SitecoreFieldAttribute attr = property.Attribute as SitecoreFieldAttribute;

            try
            {
                Guid id = Guid.Empty;
                id = new Guid(fieldValue);
                target = item.Database.GetItem(new ID(id));
            }
            catch (Exception ex)
            {
                target = item.Database.GetItem(fieldValue);
            }

            if (target == null) return null;
            bool isLazy = attr.Setting != SitecoreFieldSettings.DontLoadLazily;

            return context.CreateClass(isLazy, property.Property.PropertyType, target);
            
        }

        public override string SetFieldValue(Type returnType, object value, InstanceContext context)
        {
            return context.GetClassId(returnType, value).ToString("B");
        }

        public override Type TypeHandled
        {
            get { throw new NotImplementedException(); }
        }

        
    }
}