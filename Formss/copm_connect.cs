using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Shapes;
using Windows.Media.Protection.PlayReady;
using static Quiz_2.Formss.copm_connect;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


namespace Quiz_2.Formss
{
    public partial class copm_connect : Form
    {
        public List<ServerInfo> discoveredServers = new List<ServerInfo>();
        public TcpClient userTcpClient;
        public Thread receiveThread;
        public bool isConnected = false;
        public bool exiit = false;
        // Add a flag to check if the server is still available
        public bool isServerAvailable = true;
        public class ServerInfo
        {
            public string Name { get; set; }
            public string Address { get; set; }
        }
        public static class ControlID
        {
            public static string Comp_Names { get; set; }
            public static string connectedClients_Names { get; set; }
            public static string confi { get; set; }
            public static string connected_Server_Names { get; set; }
            public static string connected_Server_Address { get; set; }
        }
        public copm_connect()
        {
            InitializeComponent();
        }

        private void copm_connect_Load(object sender, EventArgs e)
        {

        }
        private void copm_connect_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Set flags to stop threads
                isConnected = true;
                isServerAvailable = false;
                exiit = true;
                DisconnectFromServer();
                // Wait for threads to finish
                receiveThread.Join();

                // Close the application
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in FormClosing: {ex.Message}");
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                DiscoverServers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in button4: {ex.Message}");
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            exiit = true;
            if (DisconnectFromServer())
            {
                button3.Enabled = false;
                MessageBox.Show("Disconnected from the server.");
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        this.Hide();
                        var form2 = new room();
                        form2.Closed += (s, args) => this.Show(); // Show the current form when form2 is closed
                        form2.Show();
                    }));
                }
                else
                {
                    this.Hide();
                    var form2 = new room();
                    form2.Closed += (s, args) => this.Show(); // Show the current form when form2 is closed
                    form2.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in button2: {ex.Message}");
            }
        }
        private async void BtnServer_Click(object sender, EventArgs e)
        {
            if (txt_name.Text == "")
            {
                MessageBox.Show("Please Write the team Name First"); return;
            }
            // Handle server connection logic when a server button is clicked
            if (sender is Button btn)
            {
                try
                {
                    ServerInfo serverInfo = btn.Tag as ServerInfo;
                    if (serverInfo != null)
                    {
                        ControlID.connected_Server_Names = serverInfo.Name;
                        ControlID.connected_Server_Address = serverInfo.Address;
                        // Show "Please wait, Connecting now..." message box
                        using (var waitMessageBox = new Form() { TopMost = true })
                        {
                            waitMessageBox.Size = new Size(200, 80);
                            waitMessageBox.FormBorderStyle = FormBorderStyle.FixedDialog;
                            waitMessageBox.Text = "Connecting...";
                            waitMessageBox.ControlBox = false;

                            // Center the waitMessageBox on the copm_connect form
                            waitMessageBox.StartPosition = FormStartPosition.Manual;
                            waitMessageBox.Location = new Point(this.Left + (this.Width - waitMessageBox.Width) / 2, this.Top + (this.Height - waitMessageBox.Height) / 2);

                            var label = new Label() { Text = "Please wait, Connecting now...", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                            waitMessageBox.Controls.Add(label);

                            // Show the message box
                            waitMessageBox.Show();

                            // Start the asynchronous operation to connect to the server
                            var connectTask = ConnectToServerAsync(serverInfo);

                            // Wait for the connection to complete
                            await connectTask;

                            // Dismiss the message box
                            waitMessageBox.Close();
                        }

                        receiveThread = new Thread(new ThreadStart(async () => await ReceiveAllServerMessagesAsync()));
                        receiveThread.Start();
                        btn.BackColor = Color.MediumSeaGreen;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in Connecting to the Server : {ex.Message}");
                }
            }
        }
        private async Task ConnectToServerAsync(ServerInfo serverInfo)
        {
            try
            {
                // Your existing ConnectToServer method logic
                TcpClient tcpClient = new TcpClient(serverInfo.Address, 12345);

                isConnected = true;
                isServerAvailable = true;
                button3.Enabled = true;

                string message = txt_name.Text;
                ControlID.Comp_Names = txt_name.Text;
                SendMessageToAdmin(message, serverInfo.Address);
                MessageBox.Show($"Connected to {serverInfo.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Form Error connecting to {serverInfo.Name}: {ex.Message}");
            }
        }
        private async void DiscoverServers()
        {
            // Disable the button while discovering servers
            button4.Enabled = false;
            try
            {
                discoveredServers.Clear();
                ControlID.connected_Server_Names = "";
                ControlID.connected_Server_Address = "";
                await Task.Run(() =>
                {
                    UdpClient udpClient = new UdpClient(12346);
                    IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    int x = 0;
                    while (x < 5)
                    {
                        try
                        {
                            byte[] data = udpClient.Receive(ref serverEndpoint);
                            string serverInfo = Encoding.ASCII.GetString(data);
                            AddDiscoveredServer(serverInfo);
                            x++;
                        }
                        catch (SocketException ex)
                        {
                            // Handle SocketException, e.g., if the operation was canceled
                            MessageBox.Show($"Error in mini Discovering Servers: {ex.Message}");
                            break;
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in parent Discovering Servers: {ex.Message}");
            }
            // Enable the button after discovering servers
            button4.Enabled = true;
        }
        private void AddDiscoveredServer(string serverInfo)
        {
            try
            {
                string[] parts = serverInfo.Split(':');
                if (parts.Length == 2)
                {
                    string serverName = parts[0].Trim();
                    string serverAddress = parts[1].Trim();

                    // Check if the server with the same address already exists
                    if (discoveredServers.Any(s => s.Address == serverAddress))
                    {
                        return; // Server already exists, skip adding it again
                    }

                    ServerInfo info = new ServerInfo { Name = serverName, Address = serverAddress };

                    // Update UI on the main thread
                    Invoke(new Action(() =>
                    {
                        AddServerControl(info);
                    }));

                    // Add to the list for later use when connecting
                    discoveredServers.Add(info);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in Add Discovered Servers: {ex.Message}");
            }
        }
        private void AddServerControl(ServerInfo serverInfo)
        {
            try
            {
                Button btnServer = new Button();
                btnServer.Text = serverInfo.Name;
                btnServer.Font = new System.Drawing.Font("Sakkal Majalla", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                btnServer.Tag = serverInfo; // Store ServerInfo object as Tag
                btnServer.Click += BtnServer_Click;
                Size btnServer1 = TextRenderer.MeasureText(btnServer.Text, btnServer.Font);
                btnServer.Size = new Size(btnServer1.Width + 20, btnServer1.Height + 6);
                flowLayoutPanel1.Controls.Add(btnServer);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in Add Server Control: {ex.Message}");
            }
        }
        private bool DisconnectFromServer()
        {
            try
            {
                ControlID.connected_Server_Names ="";
                ControlID.connected_Server_Address = "";
                discoveredServers.Clear();
                if ((userTcpClient != null && userTcpClient.Connected) || isServerAvailable == false)
                {
                    isConnected = false;
                    userTcpClient.GetStream().Close(); // Close the stream first
                    userTcpClient.Close();
                }

                // Wait for the threads to finish gracefully
                Thread.Sleep(3000); 

                if (!IsConnected())
                {
                    try
                    {
                        ClearFlowLayoutPanelControls();
                    }
                    catch
                    {

                    }
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                try
                {
                    ClearFlowLayoutPanelControls();
                    return true;
                }
                catch
                {
                    MessageBox.Show($"Error disconnecting from the server: {ex.Message}");
                    return false;
                }
            }
        }
        private void ClearFlowLayoutPanelControls()
        {
            try
            {
                if (flowLayoutPanel1.InvokeRequired)
                {
                    // If we're on a different thread than the UI thread, invoke the operation on the UI thread
                    flowLayoutPanel1.Invoke(new MethodInvoker(ClearFlowLayoutPanelControls));
                }
                else
                {
                    // Clear the controls on the UI thread
                    flowLayoutPanel1.Controls.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Clear Flow Layout Panel Controls: {ex.Message}");
            }
        }
        private bool IsConnected()
        {
            try
            {
                if (userTcpClient != null)
                {
                    // Check the connection state
                    return userTcpClient.Client.Connected;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public void SendMessageToAdmin(string message, string Address)
        {
            try
            {
                // Example code in copm_connect form
                userTcpClient = new TcpClient();
                userTcpClient.Connect(Address, 12345);

                if (userTcpClient != null && userTcpClient.Connected)
                {

                    NetworkStream stream = userTcpClient.GetStream();
                    StreamWriter writer = new StreamWriter(stream);

                    // Send the message to the admin
                    writer.WriteLine(message);
                    writer.Flush();
                }
                else
                {
                    MessageBox.Show("Not connected to the server.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message to admin: {ex.Message}");
            }
        }
        private async Task ReceiveAllServerMessagesAsync()
        {
            try
            {
                int cnt = 0;
                NetworkStream stream = userTcpClient.GetStream();
                StreamReader reader = new StreamReader(stream);
                string Corrupted_images = "";
                var form2 = new room();
                while (isConnected)
                {
                    try
                    {
                        // Attempt to read server alive message
                        string message = await reader.ReadLineAsync();

                        if (message != null)
                        {
                            if (message.Trim() == "MESSAGE:SERVER_ALIVE")
                            {
                                cnt = 0;
                            }
                            if (message.StartsWith("IMAGE:"))
                            {
                                cnt = 0;
                                // Split the header and data using the specified separator
                                string[] parts = message.Split(new[] { "<#>" }, StringSplitOptions.None);
                                
                                
                                if (message.Contains("Start"))
                                {
                                    if (InvokeRequired)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            form2.Hide();
                                            var form3 = new quz_tek();
                                            form3.Closed += (s, args) => this.Close(); // Show the current form when form2 is closed
                                            form3.Show();
                                        }));
                                    }
                                    else
                                    {
                                        form2.Hide();
                                        var form3 = new quz_tek();
                                        form3.Closed += (s, args) => this.Close(); // Show the current form when form2 is closed
                                        form3.Show();
                                    }
                                }
                                else if (message.Contains("Finished"))
                                {
                                    try
                                    {
                                        if(Corrupted_images != "")
                                        {
                                            SendMessageToAdmin("Corrupted:" + Corrupted_images + ControlID.Comp_Names, ControlID.connected_Server_Address);
                                        }
                                        string imageName = message.Substring("IMAGE:".Length).Trim();
                                        ControlID.confi = imageName;
                                        //button2.PerformClick();
                                        if (InvokeRequired)
                                        {
                                            Invoke(new Action(() =>
                                            {
                                                this.Hide();
                                                form2 = new room();
                                                form2.Closed += (s, args) => this.Show(); // Show the current form when form2 is closed
                                                form2.Show();
                                            }));
                                        }
                                        else
                                        {
                                            this.Hide();
                                            form2 = new room();
                                            form2.Closed += (s, args) => this.Show(); // Show the current form when form2 is closed
                                            form2.Show();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Error in Finished: {ex.Message}");
                                    }
                                }
                                else if (message.Contains("unique:"))
                                {
                                    try
                                    {
                                        string resultString = message.Replace("IMAGE:unique:", "");
                                        ControlID.connectedClients_Names = resultString;
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Error in unique: {ex.Message}");
                                    }
                                }
                                else if (parts.Length == 2 && parts[0].Contains("IMAGE:"))
                                {
                                    try
                                    {
                                        string imageName = "";
                                        try
                                        {
                                            // Get the directory path
                                            string directoryPath = Path.GetDirectoryName(Path.Combine("Temp", imageName));
                                            // Get the directory path
                                            string directoryPath2 = Path.GetDirectoryName(directoryPath);
                                            // If the directory doesn't exist, create it
                                            if (!Directory.Exists(directoryPath2))
                                            {
                                                Directory.CreateDirectory(directoryPath2);
                                            }

                                            using (var output = File.Create(directoryPath2))
                                            {
                                                // read the file divided by 1KB
                                                var buffer = new byte[1024];
                                                int bytesRead;
                                                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                                                {
                                                    output.Write(buffer, 0, bytesRead);
                                                }
                                            }

                                            // This is an image imageName
                                            imageName = parts[0].Substring("IMAGE:".Length).Trim();

                                            // This is an image imageData
                                            byte[] imageData = Convert.FromBase64String(parts[1]);
                                            SavePictureToFile(Path.Combine("Temp", imageName), imageData);
                                        }
                                        catch
                                        {
                                            Corrupted_images = Corrupted_images + imageName + "<#>";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Error in Contains (IMAGE:): {ex.Message}");
                                    }
                                }


                            }
                            if (message == "")
                            {
                                cnt = 0;
                            }
                        }
                        else
                        {
                            cnt++;
                        }
                        if (cnt > 5)
                        {
                            MessageBox.Show("Didn't Recieve any data from Server For 5 Times,Disconnecting From Server...");
                            // Server alive message not received, Disconnect and close functions
                            isServerAvailable = false; isConnected = false;
                            DisconnectFromServer();
                            break; // Exit the loop to terminate the thread
                        }
                    }
                    catch (IOException)
                    {
                        //MessageBox.Show("Error reading from the stream,Disconnecting From Server...");
                        //Error reading from the stream, disconnect and close functions
                        isServerAvailable = false; isConnected = false;
                        DisconnectFromServer();
                    }
                    finally
                    {
                        // Notify the user or perform any other UI-related operations
                        if (isServerAvailable == false && exiit == false)
                        {
                            DisconnectFromServer();
                            MessageBox.Show("Server is not Available");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ReceiveAllServerMessagesAsync: {ex.Message}");
            }
        }
        private async Task<byte[]> ReceiveImageDataAsync(NetworkStream stream)
        {
            // Receive image data length
            byte[] lengthBytes = new byte[4];
            await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
            int imageDataLength = BitConverter.ToInt32(lengthBytes, 0);

            // Receive image data
            byte[] imageData = new byte[imageDataLength];
            int bytesRead = 0;
            while (bytesRead < imageDataLength)
            {
                bytesRead += await stream.ReadAsync(imageData, bytesRead, imageDataLength - bytesRead);
            }
            return imageData;
        }
        private static void SavePictureToFile(string filePath, byte[] imageData)
        {
            try
            {
                // Get the directory path
                string directoryPath = Path.GetDirectoryName(filePath);

                // If the directory doesn't exist, create it
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Set the directory attributes to hidden and system
                // File.SetAttributes(filePath, FileAttributes.Hidden | FileAttributes.System);

                // Delete the existing file if it exists
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Write the bytes to the file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(imageData, 0, imageData.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in SavePictureToFile : {ex.Message}");
            }
        }

    }
}

   