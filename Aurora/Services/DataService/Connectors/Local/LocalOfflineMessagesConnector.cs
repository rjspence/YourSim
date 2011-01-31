using System;
using System.Collections.Generic;
using System.Text;
using Aurora.DataManager;
using Aurora.Framework;
using OpenMetaverse;
using Nini.Config;
using OpenSim.Framework;
using System.Xml;
using System.Xml.Serialization;
using OpenSim.Services.Interfaces;

namespace Aurora.Services.DataService
{
    public class LocalOfflineMessagesConnector : IOfflineMessagesConnector
	{
        private IGenericData GD = null;

        public void Initialize(IGenericData GenericData, ISimulationBase simBase, string defaultConnectionString)
        {
            IConfigSource source = simBase.ConfigSource;

            GD = GenericData;

            if (source.Configs[Name] != null)
                defaultConnectionString = source.Configs[Name].GetString("ConnectionString", defaultConnectionString);

            GD.ConnectToDatabase(defaultConnectionString);

            DataManager.DataManager.RegisterPlugin(Name+"Local", this);

            if (source.Configs["AuroraConnectors"].GetString("OfflineMessagesConnector", "LocalConnector") == "LocalConnector")
            {
                DataManager.DataManager.RegisterPlugin(Name, this);
            }
            else
            {
                //Check to make sure that something else exists
                List<string> m_ServerURI = simBase.ApplicationRegistry.RequestModuleInterface<IConfigurationService>().FindValueOf("RemoteServerURI");
                if (m_ServerURI.Count == 0) //Blank, not set up
                {
                    OpenSim.Framework.Console.MainConsole.Instance.Output("[AuroraDataService]: Falling back on local connector for " + "OfflineMessagesConnector", "None");
                    GD = GenericData;

                    if (source.Configs[Name] != null)
                        defaultConnectionString = source.Configs[Name].GetString("ConnectionString", defaultConnectionString);

                    GD.ConnectToDatabase(defaultConnectionString);

                    DataManager.DataManager.RegisterPlugin(Name, this);
                }
            }
        }

        public string Name
        {
            get { return "IOfflineMessagesConnector"; }
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Gets all offline messages for the user in GridInstantMessage format.
        /// </summary>
        /// <param name="agentID"></param>
        /// <returns></returns>
        public GridInstantMessage[] GetOfflineMessages(UUID agentID)
		{
            //Get all the messages
            List<GridInstantMessage> Messages = GenericUtils.GetGenerics<GridInstantMessage>(agentID, "OfflineMessages", GD, new GridInstantMessage());
            //Clear them out now that we have them
            GenericUtils.RemoveGeneric(agentID, "OfflineMessages", GD);
            return Messages.ToArray();
		}

        /// <summary>
        /// Adds a new offline message for the user.
        /// </summary>
        /// <param name="message"></param>
        public void AddOfflineMessage(GridInstantMessage message)
		{
            GenericUtils.AddGeneric(new UUID(message.toAgentID), "OfflineMessages", UUID.Random().ToString(), message.ToOSD(), GD);
		}
	}
}
