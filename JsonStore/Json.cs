using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

namespace JsonStore
{
    /// <summary>
    /// JSON object to store SIMPL digital, analog and string values
    /// </summary>
    public class Json
    {
        private List<string> _strings = new List<string>();
        private List<int> _integers = new List<int>();
        private List<bool> _bools = new List<bool>();
        private readonly object _fileLock = new object();
        private readonly object _listLock = new object();

        public TriListChangedCallback OnTriListChanged { get; set; }

        public FileCreatedCallback OnFileCreated { get; set; }

        public delegate void TriListChangedCallback(SimplLists simplLists);

        public delegate void FileCreatedCallback(ushort value);

        /// <summary>
        /// Sets or gets the JSON file ID
        /// </summary>
        public string ID { get {return _id;}}

        /// <summary>
        /// Sets whether the JSON file should be indented
        /// </summary>
        public ushort IndentTrue { get; set; }

        private string _id;
        private int _stringsTotal;
        private int _integersTotal;
        private int _boolsTotal;
        private bool _fileLoaded;
        private string _filePath;

        /// <summary>
        /// New or existing string to store
        /// </summary>
        /// <param name="sString">String value to store</param>
        /// <param name="index">String index in the list</param>
        public void ChangeString(string sString, ushort index)
        {
            if (!_fileLoaded)
                return;

            if (index < _strings.Count)
            {
                lock (_listLock)
                {
                    _strings[index] = sString;
                }
                ConvertListsToJson();
            }
        }

        /// <summary>
        /// New or existing strings to store
        /// </summary>
        /// <param name="sStrings">Array of string values to store</param>
        /// <param name="startIndex">Starting index in the list</param>
        /// <param name="endIndex">Ending index in the list</param>
        public void ChangeStrings(string[] sStrings, ushort startIndex, ushort endIndex)
        {
            if (!_fileLoaded)
                return;

            if (startIndex >= 0 && endIndex < _strings.Count)
            {
                if ((endIndex - startIndex) > 0)
                {
                    lock (_listLock)
                    {
                        var cnt = 0;
                        for (var i = startIndex; i <= endIndex; i++)
                        {
                            _strings[i] = sStrings[cnt];
                            cnt++;
                        }
                    }

                    ConvertListsToJson();
                }
            }
        }

        /// <summary>
        /// New or existing integer to store
        /// </summary>
        /// <param name="sInteger">Integer value to store</param>
        /// <param name="index">Integer index in the list.</param>
        public void ChangeInteger(ushort sInteger, ushort index)
        {
            if (!_fileLoaded)
                return;

            if (index < _integers.Count)
            {
                lock (_listLock)
                {
                    _integers[index] = Convert.ToInt32(sInteger);
                }
                ConvertListsToJson();
            }
        }

        /// <summary>
        /// New or existing integers to store
        /// </summary>
        /// <param name="sIntegers">Array of integer values to store</param>
        /// <param name="startIndex">Starting index in the list</param>
        /// <param name="endIndex">Ending index in the list</param>
        public void ChangeIntegers(ushort[] sIntegers, ushort startIndex, ushort endIndex)
        {
            if (!_fileLoaded)
                return;

            if (startIndex >= 0 && endIndex < _integers.Count)
            {
                if ((endIndex - startIndex) > 0)
                {
                    lock (_listLock)
                    {
                        var cnt = 0;
                        for (var i = startIndex; i <= endIndex; i++)
                        {
                            _integers[i] = sIntegers[cnt];
                            cnt++;
                        }
                    }
                    ConvertListsToJson();
                }
            }
        }

        /// <summary>
        /// New or existing bool to store
        /// </summary>
        /// <param name="sBool">Bool value to store</param>
        /// <param name="index">Bool index in the list</param>
        public void ChangeBool(ushort sBool, ushort index)
        {
            if (!_fileLoaded)
                return;

            if (index < _bools.Count)
            {
                lock (_listLock)
                {
                    _bools[index] = Convert.ToBoolean(sBool);
                }
                ConvertListsToJson();
            }
        }

        /// <summary>
        /// New or existing bools to store
        /// </summary>
        /// <param name="sBools">Array of bool values to store</param>
        /// <param name="startIndex">Starting index in the list</param>
        /// <param name="endIndex">Ending index in the list</param>
        public void ChangeBools(ushort[] sBools, ushort startIndex, ushort endIndex)
        {
            if (!_fileLoaded)
                return;

            if (startIndex >= 0 && endIndex < _bools.Count)
            {
                if ((endIndex - startIndex) > 0)
                {
                    lock (_listLock)
                    {
                        var cnt = 0;
                        for (var i = startIndex; i <= endIndex; i++)
                        {
                            _bools[i] = Convert.ToBoolean(sBools[cnt]);
                            cnt++;
                        }
                    }
                    ConvertListsToJson();
                }
            }
        }

        public void ChangeTriList(SimplLists simplList, ushort startPosition, ushort endPosition)
        {
            if (!_fileLoaded)
                return;

            if (startPosition >= 0)
            {
                if (endPosition < _strings.Count && endPosition < _integers.Count && endPosition < _bools.Count)
                {
                    if ((endPosition - startPosition) > 0)
                    {
                        lock (_listLock)
                        {
                            _strings = simplList.Strings.ToList();
                            _integers = simplList.Integers.Select(x => (int)x).ToList();
                            _bools = simplList.Bools.Select(x => x != 0).ToList();
                        }
                        ConvertListsToJson();
                    }
                }
            }
        }


        /// <summary>
        /// Load or create the JSON file
        /// </summary>
        /// <param name="stringTotal">Total strings in list</param>
        /// <param name="integerTotal">Total integers is list</param>
        /// <param name="boolTotal">Total bools in list</param>
        /// <param name="path">File directory to save file</param>
        /// <param name="id">ID to assign the JSON file</param>
        public void LoadLists(ushort stringTotal, ushort integerTotal, ushort boolTotal, string path, string id)
        {
            _id = id;
            if (path.Length > 0)
            {
                try
                { 
                    lock (_fileLock)
                    {
                        
                        _filePath = string.Format(@"{0}jsonStore_{1}.json", path, _id);

                        if (!Directory.Exists(path))
                            Directory.Create(path);
                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Exception("Exception occured in LoadLists custom directory", e);
                    return;
                }
            }
            else
            {
                try
                {
                    lock (_fileLock)
                    {
                        var currentDirectory = string.Format("{0}\\user\\", Directory.GetApplicationRootDirectory());
                        _filePath = string.Format("{0}jsonStore_{1}.json", currentDirectory, _id);

                    }
                }
                catch (Exception e)
                {
                    ErrorLog.Exception("Exception occured in LoadLists default directory", e);
                    return;
                }
            }

            _stringsTotal = stringTotal;
            _integersTotal = integerTotal;
            _boolsTotal = boolTotal;

            try
            {
                if (File.Exists(_filePath))
                {
                    lock (_fileLock)
                    {
                        using (StreamReader reader = new StreamReader(File.OpenRead(_filePath)))
                        {
                            var lists = JsonConvert.DeserializeObject<Lists>(reader.ReadToEnd());

                            lock (_listLock)
                            {
                                _strings = lists.strings;
                                _integers = lists.integers;
                                _bools = lists.bools;
                            }
                        }
                    }
                }
                else
                {
                    lock (_listLock)
                    {
                        for (var i = 1; i <= stringTotal; i++)
                        {
                            _strings.Add(string.Empty);
                        }
                        for (var i = 1; i <= integerTotal; i++)
                        {
                            _integers.Add(0);
                        }
                        for (var i = 1; i <= boolTotal; i++)
                        {
                            _bools.Add(false);
                        }
                    }

                    ConvertListsToJson();
                }

                if (OnFileCreated != null)
                    OnFileCreated(Convert.ToUInt16(File.Exists(_filePath)));
            }
            catch (Exception e)
            {
                if (OnFileCreated != null)
                    OnFileCreated(0);
                ErrorLog.Error("jsonStore_{0} Error creating/reading lists: {1}", ID, e.InnerException);
                return;
            }

            _fileLoaded = true;
            SendLists();
        }

        /// <summary>
        /// Sends digital, analog and string lists to S+
        /// </summary>
        public void SendLists()
        {
            if (!_fileLoaded)
                return;

            lock (_listLock)
            {
                OnTriListChanged(new SimplLists(_strings.ToArray(), _integers.ToArray(), _bools.ToArray()));
            }
        }

        //converts tri list to JSON and saves to file
        private void ConvertListsToJson()
        {
            try
            {
                lock (_fileLock)
                {
                    using (FileStream writer = File.Create(_filePath))
                    {
                        var json = JsonConvert.SerializeObject(new Lists() { FileID = ID, strings = _strings, integers = _integers, bools = _bools }, IndentTrue == 1 ? Formatting.Indented : Formatting.None);
                        writer.Write(json, Encoding.ASCII);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Execption in ConvertListsToJson", e);
            }
        }

        /// <summary>
        /// Sends the JSON file to another processor/FTP server
        /// </summary>
        /// <param name="host">IP address of remote FTP server</param>
        /// <param name="username">Username to login to the remote FTP server</param>
        /// <param name="password">Password to login to the remote FTP server</param>
        /// <param name="remotePath">Remote FTP server file path to upload file</param>
        public void SendToAnotherProcessor(string host, string username, string password, string remotePath)
        {

            if (!_fileLoaded)
                return;

            using (CrestronFileTransferClient client = new CrestronFileTransferClient())
            {
                client.SetVerbose(true);
                client.SetUserName(username);
                client.SetPassword(password);

                lock (_fileLock)
                {
                    try
                    {
                        var success = client.PutFile(string.Format("{0}{1}/jsonStore_{0}.json", host, remotePath, ID), _filePath);

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

    }
}
