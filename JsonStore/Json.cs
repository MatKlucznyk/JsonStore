using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace JsonStore
{
    /// <summary>
    /// Object to desearlize JSON.
    /// </summary>
    public class Lists
    {
        /// <summary>
        /// Current file ID.
        /// </summary>
        public string FileID { get; set; }

        /// <summary>
        /// List of stored strings.
        /// </summary>
        public List<string> strings { get; set; }

        /// <summary>
        /// List of stored integers.
        /// </summary>
        public List<int> integers { get; set; }

        /// <summary>
        /// List of stored bools.
        /// </summary>
        public List<bool> bools { get; set; }

        /// <summary>
        /// Returns objects value as a string.
        /// </summary>
        /// <returns>Objects value as a string</returns>
        public override string ToString()
        {
            return "strings:\n" + strings.ToString() + "\nintegers:\n" + integers.ToString() + "\nbools:\n" + bools.ToString() + "\n";
        }
    }

    /// <summary>
    /// Object to setup and use the stored lists.
    /// </summary>
    public class Json
    {
        private List<string> strings = new List<string>();
        private List<int> integers = new List<int>();
        private List<bool> bools = new List<bool>();
        
        /// <summary>
        /// Sets or gets the newOnListChange.
        /// </summary>
        public OnListChange newOnListChange { get; set; }

        /// <summary>
        /// Sets or gets the fileSuccessfullyCreated.
        /// </summary>
        public OnFileSuccesfullyCreated fileSuccessfullyCreated { get; set; }

        /// <summary>
        /// Delegate that passes data between S# and S+.
        /// </summary>
        /// <param name="s">Returned string.</param>
        /// <param name="i">Returned integer.</param>
        /// <param name="b">Returned bool.</param>
        /// <param name="type">Returned signal type.</param>
        /// <param name="position">Returned signals location in the list.</param>
        public delegate void OnListChange(SimplSharpString s, ushort i, ushort b, SimplSharpString type, ushort position);

        /// <summary>
        /// Delegate that passes file creation status to s+.
        /// </summary>
        /// <param name="value">Returns file exists status.</param>
        public delegate void OnFileSuccesfullyCreated(ushort value);

        /// <summary>
        /// Sets or gets the JSON ID.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Sets file formatting.
        /// </summary>
        public ushort IndentTrue { get; set; }

        private int stringsTotal;
        private int integersTotal;
        private int boolsTotal;

        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            if (file.Exists)
            {
                try
                {
                    stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    return true;
                }
                finally
                {
                    if (stream != null)
                        stream.Close();
                }
            }

            //file is not locked
            return false;
        }

        private string filePath = string.Empty;

        /// <summary>
        /// New or existing string to store.
        /// </summary>
        /// <param name="data">Strings contents.</param>
        /// <param name="position">Strings location in the list.</param>
        public void ChangeString(string data, ushort position)
        {
            strings[position] = data;
            ConvertListsToJson();
        }

        /// <summary>
        /// New or existing integer to store.
        /// </summary>
        /// <param name="data">Integers value.</param>
        /// <param name="position">Integers location in the list.</param>
        public void ChangeInteger(ushort data, ushort position)
        {
            integers[position] = Convert.ToInt16(data);
            ConvertListsToJson();
        }

        /// <summary>
        /// New or existing bool to store.
        /// </summary>
        /// <param name="data">Bools value.</param>
        /// <param name="position">Bools postion in the list.</param>
        public void ChangeBool(ushort data, ushort position)
        {
            bools[position] = Convert.ToBoolean(data);
            ConvertListsToJson();
        }


        /// <summary>
        /// Setups objects and creates/reads the JSON file.
        /// </summary>
        /// <param name="stringTotal">Total strings in list.</param>
        /// <param name="integerTotal">Total integers is list.</param>
        /// <param name="boolTotal">Total bools in list.</param>
        /// <param name="path">File directory to save file too.</param>
        public void LoadLists(ushort stringTotal, ushort integerTotal, ushort boolTotal, string path)
        {
            if (path.Length > 0)
            {
                try
                {
                    filePath = path + string.Format(@"\jsonStore_{0}.json", ID);

                    if (!Directory.Exists(path))
                        Directory.Create(path);
                }
                catch (Exception e)
                {
                    ErrorLog.Exception("Exception occured in LoadLists custom directory", e);
                }
            }
            else
            {
                try
                {
                    var currentDirectory = Directory.GetApplicationDirectory().Split('\\');
                    filePath = string.Format(@"\User\{0}\jsonStore_{1}.json", currentDirectory[2], ID);

                    if (!Directory.Exists(string.Format(@"\User\{0}\", currentDirectory[2])))
                        Directory.Create(string.Format(@"\User\{0}\", currentDirectory[2]));
                }
                catch (Exception e)
                {
                    ErrorLog.Exception("Exception occured in LoadLists default directory", e);
                }
            }

            stringsTotal = stringTotal;
            integersTotal = integerTotal;
            boolsTotal = boolTotal;

            try
            {
                if (File.Exists(filePath))
                {
                    while (IsFileLocked(new FileInfo(filePath))) ;
                    using (StreamReader reader = new StreamReader(File.OpenRead(filePath)))
                    {
                        Lists lists = JsonConvert.DeserializeObject<Lists>(reader.ReadToEnd());

                        strings = lists.strings;
                        integers = lists.integers;
                        bools = lists.bools;
                    }
                }

                else
                {
                    

                    for (int i = 1; i <= stringTotal; i++)
                    {
                        strings.Add(string.Empty);
                    }
                    for (int i = 1; i <= integerTotal; i++)
                    {
                        integers.Add(0);
                    }
                    for (int i = 1; i <= boolTotal; i++)
                    {
                        bools.Add(false);
                    }

                    ConvertListsToJson();
                }

                if (fileSuccessfullyCreated != null && File.Exists(filePath))
                    fileSuccessfullyCreated(1);
                else if (fileSuccessfullyCreated != null && !File.Exists(filePath))
                    fileSuccessfullyCreated(0);
            }
            catch (Exception e)
            {
                if (fileSuccessfullyCreated != null)
                    fileSuccessfullyCreated(0);
                ErrorLog.Error("jsonStore_{0} Error creating/reading lists: {1}", ID, e.InnerException);
            }

            SendLists();
        }

        /// <summary>
        /// Sends all lists to S+.
        /// </summary>
        private void SendLists()
        {
            for(int i = 1; i <= stringsTotal; i++)
            {
                newOnListChange(strings[i - 1], Convert.ToUInt16(strings[i - 1].Length), 1, "string", Convert.ToUInt16(i));
            }
            for (int i = 1; i <= integersTotal; i++)
            {
                newOnListChange(integers[i - 1].ToString(), Convert.ToUInt16(integers[i - 1]), Convert.ToUInt16(Convert.ToBoolean(integers[i - 1])), "int", Convert.ToUInt16(i));
            }
            for (int i = 1; i <= boolsTotal; i++)
            {
                newOnListChange(bools[i - 1].ToString(), Convert.ToUInt16(bools[i - 1]), Convert.ToUInt16(bools[i - 1]), "bool", Convert.ToUInt16(i));
            }
        }

        //converts tri list to JSON and saves to file
        private void ConvertListsToJson()
        {
            try
            {
                Lists lists = new Lists();
                lists.FileID = ID;
                lists.strings = strings;
                lists.integers = integers;
                lists.bools = bools;

                string json = string .Empty;

                if (IndentTrue == 0)
                    json = JsonConvert.SerializeObject(lists, Formatting.None);
                else
                    json = JsonConvert.SerializeObject(lists, Formatting.Indented);

                while (IsFileLocked(new FileInfo(filePath))) ;
                using (FileStream writer = File.Create(filePath))
                {
                    writer.Write(json, Encoding.ASCII);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Execption in ConvertListsToJson", e);
            }
        }

        /// <summary>
        /// Sends the JSON file to another processor/FTP server.
        /// </summary>
        /// <param name="ipAddress">IP address of FTP server.</param>
        /// <param name="username">Username to login to the FTP server.</param>
        /// <param name="password">Password to login to the FTP server.</param>
        /// <param name="remotePath">File path to upload file too.</param>
        public void SendToAnotherProcessor(string host, string username, string password, string remotePath)
        {
            try
            {
                CrestronFileTransferClient client = new CrestronFileTransferClient();

                client.SetVerbose(true);
                client.SetUserName(username);
                client.SetPassword(password);

                //if (type == "ftp")
                var success = client.PutFile(string.Format("{0}{1}/jsonStore_{0}.json", host, remotePath, ID), filePath);

                if (success == -1 || success == 1)
                {
                    string err = client.GetLastClientStrError();
                    ErrorLog.Error("jsonStore_{0} Error Sending File: {1}", ID, err);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Exception in SendToAnotherProcessor", e);
            }
        }
    }
}
