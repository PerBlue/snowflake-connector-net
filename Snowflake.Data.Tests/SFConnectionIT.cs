﻿/*
 * Copyright (c) 2012-2017 Snowflake Computing Inc. All rights reserved.
 */

namespace Snowflake.Data.Tests
{
    using NUnit.Framework;
    using Snowflake.Data.Client;
    using System.Data;

    [TestFixture]
    class SFConnectionIT : SFBaseTest
    {
        [Test]
        public void testLoginTimeout()
        {
            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                string invalidConnectionString = "host=invalidaccount.snowflakecomputing.com;connection_timeout=30;"
                    + "account=invalidaccount;user=snowman;password=test;";

                conn.ConnectionString = invalidConnectionString;

                Assert.AreEqual(conn.State, ConnectionState.Closed);
                try
                {
                    conn.Open();
                    Assert.Fail();
                }
                catch(SnowflakeDbException e)
                {
                    Assert.AreEqual(270007, e.ErrorCode);
                }
            }

        }

        [Test]
        public void testInvalidConnectioinString()
        {
            string[] invalidStrings = {
                // missing required connection property password
                "ACCOUNT=testaccount;user=testuser",
                // invalid account value
                "ACCOUNT=A=C;USER=testuser;password=123",
                "complete_invalid_string",
            };

            int[] expectedErrorCode = { 270006, 270008, 270008 };

            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                for (int i=0; i<invalidStrings.Length; i++)
                {
                    try
                    {
                        conn.ConnectionString = invalidStrings[i];
                        conn.Open();
                        Assert.Fail();
                    }
                    catch (SnowflakeDbException e)
                    {
                        Assert.AreEqual(expectedErrorCode[i], e.ErrorCode);
                    }
                }
            }
        }

        [Test]
        public void testUnknownConnectionProperty()
        {
            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                // invalid propety will be ignored.
                conn.ConnectionString = connectionString += ";invalidProperty=invalidvalue;";

                conn.Open();
                Assert.AreEqual(conn.State, ConnectionState.Open);
                conn.Close();
            }
        }

        [Test]
        public void testSwitchDb()
        {
            using (IDbConnection conn = new SnowflakeDbConnection())
            {
                conn.ConnectionString = connectionString;

                Assert.AreEqual(conn.State, ConnectionState.Closed);

                conn.Open();
                Assert.AreEqual("TESTDB_DOTNET", conn.Database);
                Assert.AreEqual(conn.State, ConnectionState.Open);

                conn.ChangeDatabase("SNOWFLAKE_SAMPLE_DATA");
                Assert.AreEqual("SNOWFLAKE_SAMPLE_DATA", conn.Database);

                conn.Close();
            }

        }
    }
}
