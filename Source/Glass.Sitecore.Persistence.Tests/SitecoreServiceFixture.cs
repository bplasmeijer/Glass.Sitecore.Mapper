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
using NUnit.Framework;
using Glass.Sitecore.Persistence.Configuration;
using Glass.Sitecore.Persistence.Tests.SitecoreServiceFixtureNS;
using Glass.Sitecore.Persistence.Configuration.Attributes;
using Glass.Sitecore.Persistence.Data;
using Sitecore.Data.Items;
using Sitecore.Data;
using Sitecore.SecurityModel;

namespace Glass.Sitecore.Persistence.Tests
{
    [TestFixture]
    public class SitecoreServiceFixture
    {
        SitecoreService _sitecore;
        Context _context;
        Database _db;

        [SetUp]
        public void Setup()
        {

            AttributeConfigurationLoader loader = new AttributeConfigurationLoader(
                new string []{"Glass.Sitecore.Persistence.Tests.SitecoreServiceFixtureNS, Glass.Sitecore.Persistence.Tests"}
                );

            _context = new Context(loader, new ISitecoreDataHandler[]{new SitecoreIdDataHandler() });

            _sitecore = new SitecoreService("master");
            _db = global::Sitecore.Configuration.Factory.GetDatabase("master");
        }

        #region GetItem
        [Test]
        public void GetItem_ByPath_ReturnsItem()
        {
            //Assign
            string path =  "/sitecore/content/Glass/Test1";
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(path);

            //Assert
            Assert.IsNotNull(result);
        }
        [Test]
        public void GetItem_ById_ReturnsItem()
        {
            //Assign
            Guid id = new Guid("{8A317CBA-81D4-4F9E-9953-64C4084AECCA}");

            //Act
            TestClass result = _sitecore.GetItem<TestClass>(id);

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetItem_ByPath_ReturnsNullItemDoesNotExist()
        {
            //Assign
            string path = "/sitecore/content/Glass/DoesntExist";
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(path);

            //Assert
            Assert.IsNull(result);
        }
        [Test]
        public void GetItem_ById_ReturnsNullItemDoesNotExist()
        {

            //Assign
            Guid id = new Guid("{99317CBA-81D4-4F9E-9953-64C4084AECC1}");
           
            //Act
            TestClass result = _sitecore.GetItem<TestClass>(id);

            //Assert
            Assert.IsNull(result);
        }
        #endregion

        #region Query

        [Test]
        public void Query_ReturnsSetOfClasses()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(parent.Children.Count, result.Count());
            Assert.IsTrue(parent.Children.Cast<Item>().All(x => result.Any(y => y.Id == x.ID.Guid)));

        }

        [Test]
        public void Query_IncorrectQuery_ReturnsEmptyEnumeration()
        {
            //Assign
            string query = "/sitecore/content/glass/notthere/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(0, result.Count());
        }

        [Test]
        public void Query_ReturnsSetOfClasses_Proxies()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.Query<SitecoreServiceFixtureNS.TestClass>(query, true);

            //Asserte
            Assert.AreNotEqual(typeof(SitecoreServiceFixtureNS.TestClass), result.First().GetType());
            Assert.IsTrue(result.First() is SitecoreServiceFixtureNS.TestClass);
            Assert.AreEqual(parent.Children.Count, result.Count());
            Assert.IsTrue(parent.Children.Cast<Item>().All(x => result.Any(y => y.Id == x.ID.Guid)));

        }

        

        #endregion

        #region QuerySingle

        [Test]
        public void QuerySingle_ReturnsSingleClasses()
        {
            //Assign
            Item parent = _db.GetItem("/sitecore/content/glass");
            string query = "/sitecore/content/glass/*";

            //Act
            var result = _sitecore.QuerySingle<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.AreEqual(parent.Children[0].ID.Guid, result.Id);

        }
        [Test]
        public void QuerySingle_IncorrectQuery_ReturnsNull()
        {
            //Assign
            string query = "/sitecore/content/glass/notthere/*";

            //Act
            var result = _sitecore.QuerySingle<SitecoreServiceFixtureNS.TestClass>(query);

            //Assert
            Assert.IsNull(result);
        }


        #endregion

        #region Create

        [Test]
        [ExpectedException(typeof(PersistenceException))]
        public void Create_NoTemplateId_ThrowsException()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");

            using (new SecurityDisabler())
            {
                //Act
                _sitecore.Create<TestClass>(test3, "Test4");


                //Assert
                //N/A
            }
        }

        [Test]
        public void Create_CreatesAnItem()
        {
            //Assign
            TestClass test3 = _sitecore.GetItem<TestClass>("/sitecore/content/Glass/Test1/Test3");

            using (new SecurityDisabler())
            {
                //Act
                CreateClass newItem = _sitecore.Create<CreateClass>(test3, "Test4");


                //Assert
                Item item = _db.GetItem("/sitecore/content/Glass/Test1/Test3/Test4");
                Assert.IsNotNull(item);
                Assert.AreNotEqual(item.ID, newItem.Id);

                try
                {
                    //Clean up
                    item.Delete();
                }
                catch (NullReferenceException ex)
                {
                    //this expection is thrown by Sitecore.Tasks.ItemEventHandler.OnItemDeleted
                }
            }
        }

        #endregion
    }
    namespace SitecoreServiceFixtureNS
    {
        [SitecoreClass]
        public class TestClass
        {
            [SitecoreId]
            public virtual Guid Id { get; set; }
        }

        [SitecoreClass(TemplateId = "{1D0EE1F5-21E0-4C5B-8095-EDE2AF3D3300}")]
        public class CreateClass
        {
            [SitecoreId]
            public virtual Guid Id { get; set; }
        }
        

    }
}
