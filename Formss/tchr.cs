using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;

namespace Quiz_2.Formss
{
    public partial class Tchr : Form
    {
        public static List<PictureBox> selectedPictureBoxes = new List<PictureBox>();
        public List<string> selectedClients = new List<string>();

        private bool shouldStopBroadcasting = false;
        private Thread broadcastThread;
        public Thread listenerThread;

        //public int u_id = new int();
        public DataTable searssh = new DataTable();
        public TcpListener tcpListener { get; set; }

        public List<ClientInfo> connectedClients = new List<ClientInfo>();

        //private string[] timeeary;
        //private string[] Questionss;

        string result = "";
        string coll = "";
        public string Server_name = "";
        public class ClientInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public TcpClient Client { get; set; }
        }

        public Tchr()
        {
            InitializeComponent();
        }
        private void comp_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new Copms();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void strt_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new tchr_strt_quiz();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void qshns_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new Questions();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void stng_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new Login();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void rprt_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new tchr_strt_quiz();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void ext_Click(object sender, EventArgs e)
        {
            //this.Hide();
            //var form2 = new quz_tek(this);
            //form2.Closed += (s, args) => this.Close();
            //form2.Show();
            //DialogResult dg = MessageBox.Show("Exit ?", "Confirmation", MessageBoxButtons.YesNo);
            //if (dg == DialogResult.Yes)
            //{
            //    Application.Exit();
            //}
        }

        private void Tchr_Load(object sender, EventArgs e)
        {
            tabControl1.TabPages.Remove(tabPage1);
            //strt.Enabled = true;
            return;
            if (selectedPictureBoxes != null)
            {
                if (selectedPictureBoxes.Count > 0)
                {
                    qshns.BackColor = Color.MediumSeaGreen;
                }
            }
            if (selectedClients != null)
            {
                if (selectedClients.Count > 0)
                {
                    comp.BackColor = Color.MediumSeaGreen;
                }
            }
            if (selectedPictureBoxes != null && selectedClients != null)
            {
                if (selectedPictureBoxes.Count > 0 && selectedClients.Count > 0)
                {
                    strt.BackColor = Color.MediumSeaGreen;
                    strt.Enabled = true;
                }
                else
                {
                    strt.BackColor = Color.LightCoral;
                    strt.Enabled = false;
                }
            }
            else
            {
                strt.BackColor = Color.LightCoral;
                strt.Enabled = false;
            }

        }
        private void Tchr_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Set flags to stop threads
            shouldStopBroadcasting = true;

            ShutdownServer();

            if (listenerThread != null)
            {
                // Wait for threads to finish
                listenerThread.Join();
            }
            if (broadcastThread != null)
            {
                broadcastThread.Join();
            }
            // Close the application
            Application.Exit();
        }

        #region Questions
        private void button3_Click(object sender, EventArgs e)
        {
            string QuestionsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions");

            if (!Directory.Exists(QuestionsFolderPath))
            {
                Directory.CreateDirectory(QuestionsFolderPath);
                MessageBox.Show("Questions folder not found in app directory.", "Questions folder has been Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (string imageFileName in Directory.GetFiles(QuestionsFolderPath, "*.jpg", SearchOption.AllDirectories)
                                                   .Union(Directory.GetFiles(QuestionsFolderPath, "*.png", SearchOption.AllDirectories))
                                                   .Union(Directory.GetFiles(QuestionsFolderPath, "*.jpeg", SearchOption.AllDirectories)))
            {
                Image image = Image.FromFile(imageFileName);
                PictureBox pictureBox = new PictureBox();
                pictureBox.Image = image;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Size = new Size(200, 200);
                pictureBox.Click += pictureBox1_Click; // Add click event for selecting images

                // Extract the filename without extension and set it as PictureBox name
                string imageName = Path.GetFileNameWithoutExtension(imageFileName);
                pictureBox.Name = imageName;

                CheckBox checkBox = new CheckBox();
                checkBox.Name = "selectionCheckbox";
                checkBox.Visible = false; // Initially hide the checkbox
                checkBox.Location = new Point(0, 0);
                checkBox.Size = new Size(100, 30);
                checkBox.Text = "Selected";
                checkBox.ForeColor = Color.Red;
                checkBox.Font = new Font("Arial", 12F, FontStyle.Bold);
                pictureBox.Controls.Add(checkBox);
                flowLayoutPanel1.Controls.Add(pictureBox);
            }

            if (flowLayoutPanel1.Controls.Count == 0)
            {
                MessageBox.Show("Questions folder is empty", "No Questions Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            PictureBox clickedPictureBox = (PictureBox)sender;
            CheckBox checkBox = (CheckBox)clickedPictureBox.Controls["selectionCheckbox"];

            if (checkBox.Checked)
            {
                // Deselect the image
                checkBox.Visible = false;
                selectedPictureBoxes.Remove(clickedPictureBox);
                checkBox.Checked = false;
                clickedPictureBox.BorderStyle = BorderStyle.None;
            }
            else
            {
                // Select the image
                checkBox.Visible = true;
                selectedPictureBoxes.Add(clickedPictureBox);
                checkBox.Checked = true;
                clickedPictureBox.BorderStyle = BorderStyle.FixedSingle;
                clickedPictureBox.BackColor = Color.MediumSeaGreen;

            }
        }
        #endregion

        #region Compititors
        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Please Enter Server Name First");
                return;
            }
            selectedClients = new List<string>();
            if (!IsServerRunning())
            {
                label16.ForeColor = Color.MediumSeaGreen;
                label16.Text = "Server " + textBox1.Text + " Started";

                label16.AutoSize = false;

                // Calculate the center of the parent control
                int centerX = (this.ClientSize.Width - label16.Width) / 2;
                int centerY = (this.ClientSize.Height - label16.Height) / 2;

                // Set the label's location to the calculated center
                label16.Location = new Point(centerX, centerY);

                button6.Enabled = true;
                button7.Enabled = false;
                button7.Text = "Server Started...";
                flowLayoutPanel2.Controls.Clear();
                StartServer();
            }
            else
            {
                MessageBox.Show("Server is already running");
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            ShutdownServer();
            // MessageBox.Show("Server shutdown successfully.");
            button7.Enabled = true;
            button6.Enabled = false;
            label16.ForeColor = Color.Red;
            label16.Text = "Server Stopped";
            label16.AutoSize = false;

            // Calculate the center of the parent control
            int centerX = (this.ClientSize.Width - label16.Width) / 2;
            int centerY = (this.ClientSize.Height - label16.Height) / 2;

            // Set the label's location to the calculated center
            label16.Location = new Point(centerX, centerY);
            button7.Text = "Start Server";
        }
        private void lbl_selectedClients_Click(object sender, EventArgs e)
        {
            Label checkBox2 = (Label)sender;
            //checkBox2.BackColor = checkBox2.Checked ? Color.MediumSeaGreen : SystemColors.Control; // Update background color
            if (checkBox2.BackColor == Color.LightBlue)
            {
                checkBox2.BackColor = Color.White;
            }
            else
            {
                checkBox2.BackColor = Color.LightBlue;
            }
            selectedClients.Clear(); // Clear existing data
            foreach (Control control in flowLayoutPanel2.Controls)
            {
                Label checkBox = control as Label;
                if (checkBox != null && checkBox.BackColor == Color.LightBlue)
                {
                    ClientInfo clientInfo = (ClientInfo)checkBox.Tag;
                    string clientData = $"{clientInfo.Id}<#>{clientInfo.Name.Replace("\r\n", "")}<#>{clientInfo.Address}";
                    selectedClients.Add(clientData);
                }
            }
            update_ready_panel();

            #region backup
            //CheckBox checkBox2 = (CheckBox)sender;
            //checkBox2.BackColor = checkBox2.Checked ? Color.MediumSeaGreen : SystemColors.Control; // Update background color

            //selectedClients.Clear(); // Clear existing data
            //foreach (Control control in flowLayoutPanel2.Controls)
            //{
            //    CheckBox checkBox = control as CheckBox;
            //    if (checkBox != null && checkBox.Checked)
            //    {
            //        ClientInfo clientInfo = (ClientInfo)checkBox.Tag;
            //        string clientData = $"{clientInfo.Id}<#>{clientInfo.Name}<#>{clientInfo.Address}";
            //        selectedClients.Add(clientData);
            //    }
            //} 
            #endregion
        }
        private void StartServer()
        {
            shouldStopBroadcasting = false;
            tcpListener = new TcpListener(IPAddress.Any, 12345);

            // Start listen server presence

            listenerThread = new Thread(new ThreadStart(ListenForClients));
            listenerThread.Start();

            // Start broadcasting server presence
            broadcastThread = new Thread(new ThreadStart(BroadcastServerPresence));
            broadcastThread.Start();

            // Start sending server alive message
            Thread aliveMessageThread = new Thread(new ThreadStart(SendServerAliveMessage));
            aliveMessageThread.Start();

            //MessageBox.Show("Server started!");
        }
        private void ListenForClients()
        {
            tcpListener.Start();
            try
            {
                while (!shouldStopBroadcasting)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(client);

                    // Delay for 5 seconds
                    Thread.Sleep(5000);
                }
            }
            catch
            {

            }
        }
        private void BroadcastServerPresence()
        {
            UdpClient udpClient = new UdpClient();
            {
                IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 12346);
                while (!shouldStopBroadcasting)
                {
                    string serverInfo = "";
                    try
                    {
                        if (textBox1.Text == "")
                        {
                            serverInfo = "Server: " + Dns.GetHostName();
                        }
                        else
                        {
                            Server_name = textBox1.Text;
                            serverInfo = textBox1.Text + ": " + Dns.GetHostName(); // Use textBox1 for server name
                        }
                    }
                    catch
                    {

                    }
                    byte[] data = Encoding.ASCII.GetBytes(serverInfo);
                    udpClient.Send(data, data.Length, broadcastEndpoint);
                    Thread.Sleep(5000); // Broadcast every 5 seconds (adjust as needed)
                }
            }
        }
        private void HandleClientComm(object clientObj)
        {
            TcpClient tcpClient = (TcpClient)clientObj;
            NetworkStream clientStream = tcpClient.GetStream();
            string userName = "";
            // Get client information (you can customize this part)
            string clientId = Guid.NewGuid().ToString();
            byte[] message = new byte[4096];
            int bytesRead;
            try
            {
                while (!shouldStopBroadcasting)
                {
                    bytesRead = 0;
                    try
                    {
                        bytesRead = clientStream.Read(message, 0, 4096);
                    }
                    catch
                    {
                        // break;
                    }

                    if (bytesRead == 0)
                    {
                        // break; // Client has disconnected
                    }

                    string receivedMessage = Encoding.ASCII.GetString(message, 0, bytesRead);

                    if (receivedMessage != "" && !receivedMessage.Contains("Ready"))
                    {
                        userName = receivedMessage;
                        // Check if a client with the same name already exists
                        if (!connectedClients.Any(c => c.Name == userName))
                        {
                            // Add the client to the connectedClients list
                            connectedClients.Add(new ClientInfo { Id = clientId, Name = userName, Client = tcpClient });

                            // Update UI on the main thread
                            Invoke(new Action(() =>
                            {
                                UpdateUI();
                            }));
                        }
                    }
                    else if (receivedMessage.Contains("Ready"))
                    {
                        string[] parts = receivedMessage.Split(':');
                        string clientName = parts[1];
                        bool labelFound = false;

                        // Use Invoke to ensure the UI update is done on the UI thread
                        flowLayoutPanel4.Invoke((MethodInvoker)delegate
                        {
                            // Iterate through each panel in flowLayoutPanel4
                            foreach (Control control in flowLayoutPanel4.Controls)
                            {
                                if (control is Panel panel)
                                {
                                    // Find the label in the current panel
                                    Label label = panel.Controls.OfType<Label>().FirstOrDefault();
                                    PictureBox pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();

                                    if (label != null && pictureBox != null && clientName.Contains(label.Text))
                                    {
                                        // Update the PictureBox in the existing panel
                                        labelFound = true;
                                        // Assuming "ready.png" is an image in your application resources
                                        pictureBox.Image = Properties.Resources.pngegg__3_;
                                        break;
                                    }
                                }
                            }

                            // If the label was not found, create a new panel
                            if (!labelFound)
                            {
                                // Create a new panel
                                Panel newPanel = new Panel();
                                newPanel.Size = new Size(819, 60);

                                // Create a new label
                                Label newLabel = new Label();
                                newLabel.Text = clientName; // Use clientInfo.Name here
                                newLabel.Location = new Point(65, 9);
                                newLabel.Font = new System.Drawing.Font("Sakkal Majalla", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                                // Create a new picture box and set the image
                                PictureBox pictureBox = new PictureBox();
                                pictureBox.Image = Properties.Resources.pngegg__3_;
                                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                pictureBox.Location = new Point(10, 6);
                                pictureBox.Size = new Size(40, 40);

                                // Add label and picture box to the new panel
                                newPanel.Controls.Add(newLabel);
                                newPanel.Controls.Add(pictureBox);

                                // Add the new panel to flowLayoutPanel4
                                flowLayoutPanel4.Controls.Add(newPanel);
                            }



                            int counter = 0;
                            int ready = 0; ;
                            foreach (Control control in flowLayoutPanel4.Controls)
                            {
                                if (control is Panel panel)
                                {
                                    PictureBox pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();
                                    if (pictureBox.Image != Properties.Resources.pngegg__3_)
                                    {
                                        ready++;
                                    }
                                    counter++;
                                }
                            }
                            label52.Text = ready.ToString() + "/" + counter.ToString();
                            if (ready == counter)
                            {
                                button1.Enabled = true;
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions (optional)
                MessageBox.Show($"Error handling client communication: {ex.Message}");
            }
            finally
            {
                // Client has disconnected, remove it from connectedClients list
                var disconnectedClient = connectedClients.FirstOrDefault(c => c.Client == tcpClient);
                if (disconnectedClient != null)
                {
                    connectedClients.Remove(disconnectedClient);
                }

                try
                {
                    // Update UI on the main thread
                    Invoke(new Action(() =>
                    {
                        UpdateUI();
                    }));
                }
                catch (Exception)
                {

                }

                // Close the TCP client
                tcpClient.Close();
            }
        }
        private void update_ready_panel()
        {
            // Find the label in flowLayoutPanel4 in selectedClients If not ready delete
            foreach (Control control in flowLayoutPanel4.Controls)
            {
                if (control is Panel panel)
                {
                    Label label = panel.Controls.OfType<Label>().FirstOrDefault();
                    bool foundMatch = false;
                    foreach (string namesss in selectedClients)
                    {
                        if (namesss.Trim().Contains(label.Text.Trim()))
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                    if (!foundMatch)
                    {
                        // If not ready, delete the panel
                        panel.Dispose();
                    }
                }
            }
            // Find the label and pictureBox in flowLayoutPanel4 in selectedClients If not ready delete
            foreach (string clientData in selectedClients)
            {
                // Split the clientData string using "<#>" as the delimiter
                string[] clientInfoArray = clientData.Split(new string[] { "<#>" }, StringSplitOptions.None);

                // Check if the clientInfoArray has at least three elements
                if (clientInfoArray.Length >= 3)
                {
                    // Extract clientName pieces of information
                    string clientName = clientInfoArray[1];
                    bool flagg = true;

                    foreach (Control control in flowLayoutPanel4.Controls)
                    {
                        if (control is Panel panel)
                        {
                            // Find the label in flowLayoutPanel4 
                            Label label = panel.Controls.OfType<Label>().FirstOrDefault();
                            PictureBox pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();

                            if (label != null && label.Text == clientName)
                            {
                                flagg = false;
                            }
                        }
                    }
                    if(flagg)
                    {
                        // Create a new panel
                        Panel newPanel = new Panel();
                        newPanel.Size = new Size(819, 60);

                        // Create a new label
                        Label newLabel = new Label();
                        newLabel.Text = clientName; // Use clientInfo.Name here
                        newLabel.Location = new Point(65, 9);
                        newLabel.Font = new System.Drawing.Font("Sakkal Majalla", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                        // Create a new picture box and set the image
                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Image = Properties.Resources.pngegg__4_;
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Location = new Point(10, 6);
                        pictureBox.Size = new Size(40, 40);

                        // Add label and picture box to the new panel
                        newPanel.Controls.Add(newLabel);
                        newPanel.Controls.Add(pictureBox);

                        // Add the new panel to flowLayoutPanel4
                        flowLayoutPanel4.Controls.Add(newPanel);
                    }
                }
            }

            int counter = 0;
            int ready = 0; ;
            foreach (Control control in flowLayoutPanel4.Controls)
            {
                if (control is Panel panel)
                {
                    PictureBox pictureBox = panel.Controls.OfType<PictureBox>().FirstOrDefault();
                    if (pictureBox.Image == Properties.Resources.pngegg__3_)
                    {
                        ready++;
                    }
                    counter++;
                }
            }
            label52.Text = ready.ToString() + "/" + counter.ToString();
            if (ready == counter)
            {
                button1.Enabled = true;
            }
        }
        private void UpdateUI()
        {
            // Clear existing controls in FlowLayoutPanel
            flowLayoutPanel2.Controls.Clear();

            // Add new controls for each connected client, enabling multi-selection
            foreach (var client in connectedClients)
            {
                Label checkBox = new Label();
                checkBox.Font = new System.Drawing.Font("Sakkal Majalla", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                checkBox.AutoSize = false; 
                checkBox.Text = ($"{client.Name}").Trim();
                Size textSize = TextRenderer.MeasureText(checkBox.Text, checkBox.Font);
                checkBox.Size = new Size(textSize.Width + 20, textSize.Height + 6);
                checkBox.Tag = client; // Store client object for later access
                checkBox.TextAlign = ContentAlignment.MiddleCenter;

                // Check if the client is in selectedClients
                if (selectedClients.Any(selectedClient => selectedClient.Contains(client.Id)))
                {
                    checkBox.BackColor = Color.LightBlue;
                    //checkBox.BackColor = checkBox.Checked ? Color.MediumSeaGreen : SystemColors.Control; // Update background color
                }

                checkBox.Click += lbl_selectedClients_Click; // Attach event handler

                flowLayoutPanel2.Controls.Add(checkBox);


                #region backup
                //CheckBox checkBox = new CheckBox();
                //// Increase the size of the checkbox box
                //// checkBox.ClientSize = new Size(50, 50);
                //checkBox.Font = new System.Drawing.Font("Sakkal Majalla", 20f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                //checkBox.Text = $"{client.Name}";
                //Size textSize = TextRenderer.MeasureText(checkBox.Text, checkBox.Font);
                //checkBox.Size = new Size(textSize.Width + 20, textSize.Height + 6);
                //checkBox.Tag = client; // Store client object for later access


                //// Check if the client is in selectedClients
                //if (selectedClients.Any(selectedClient => selectedClient.Contains(client.Id)))
                //{
                //    checkBox.Checked = true;
                //    checkBox.BackColor = checkBox.Checked ? Color.MediumSeaGreen : SystemColors.Control; // Update background color
                //}

                //checkBox.CheckedChanged += CheckBox_CheckedChanged; // Attach event handler

                //flowLayoutPanel2.Controls.Add(checkBox); 
                #endregion
            }
        }
        private bool IsServerRunning()
        {
            try
            {
                // Try to bind to the server's address and port
                TcpListener testListener = new TcpListener(IPAddress.Any, 12345);
                testListener.Start();
                testListener.Stop();

                // If binding is successful, no server is running
                return false;
            }
            catch (SocketException)
            {
                // If binding fails, another process is using the address and port
                return true;
            }
        }
        private void ShutdownServer()
        {
            try
            {
                shouldStopBroadcasting = true; // Signal the thread to stop

                try
                {
                    if (tcpListener != null)
                    {
                        // Stop listening for new clients
                        tcpListener.Stop();
                    }

                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Error  Stop listening for new clients: {ex.Message}");
                }
                // Disconnect existing clients gracefully
                foreach (TcpClient client in connectedClients.Select(c => c.Client))
                {
                    try
                    {
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        // Log or handle exception if client disconnection fails
                        //MessageBox.Show($"Error disconnecting client: {ex.Message}");
                    }
                }
                try
                {
                    // Check if the broadcastThread is active before waiting for it to finish
                    if (broadcastThread != null && broadcastThread.IsAlive)
                    {
                        broadcastThread.Join(); // Wait for it to finish gracefully
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Error Waiting to finish: {ex.Message}");
                }
                try
                {
                    // Clear the client list
                    connectedClients = new List<ClientInfo>();
                    selectedClients = new List<string>();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Clearing: {ex.Message}");
                }
                try
                {
                    // Update UI to reflect server shutdown
                    Invoke(new Action(() =>
                    {
                        UpdateUI();
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error Updating UI to reflect server shutdown: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error shutting down server: {ex.Message}");
            }
        }
        private void SendMessageToUser(string message, TcpClient userTcpClient)
        {
            try
            {
                NetworkStream stream = userTcpClient.GetStream();
                StreamWriter writer = new StreamWriter(stream);

                // Send the message to the user
                writer.WriteLine($"MESSAGE:{message}\n");
                writer.Flush();
            }
            catch (Exception ex)
            {
                // Remove the user from connectedClients and selectedClients using the client's name
                ClientInfo clientToRemove = connectedClients.FirstOrDefault(client => client.Client == userTcpClient);
                string userNameToRemove = connectedClients.FirstOrDefault(client => client.Client == userTcpClient)?.Name;
                MessageBox.Show(userNameToRemove.Replace("\r\n", "").Trim() + " Has been Disconnected");
                if (!string.IsNullOrEmpty(userNameToRemove))
                {
                    selectedClients.RemoveAll(name => name.Trim().Contains(userNameToRemove.Trim()));
                }
                if (clientToRemove != null)
                {
                    connectedClients.Remove(clientToRemove);
                }
                // Update UI on the main thread
                Invoke(new Action(() =>
                {
                    UpdateUI();
                }));                
                // Update UI on the main thread
                Invoke(new Action(() =>
                {
                    update_ready_panel();
                }));
            }
        }
        private void SendServerAliveMessage()
        {
            while (!shouldStopBroadcasting)
            {
                try
                {
                    Thread.Sleep(5000); // Send the message every 5 seconds (adjust as needed)

                    string serverAliveMessage = "SERVER_ALIVE";

                    // Iterate through connected clients and send the message
                    foreach (var clientInfo in connectedClients)
                    {
                        SendMessageToUser($"{serverAliveMessage}", clientInfo.Client);
                    }
                }
                catch (Exception)
                {

                   
                }
            }
        }
        #endregion


        #region Start_Quiz
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Enabled = radioButton1.Checked;
            label5.Enabled = radioButton1.Checked;
            textBox3.Text = "";
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Enabled = radioButton2.Checked;
            label3.Enabled = radioButton2.Checked;
            textBox2.Text = "";
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            textBox5.Enabled = radioButton6.Checked;
            label10.Enabled = radioButton6.Checked;
            textBox5.Text = "";
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            textBox4.Enabled = radioButton5.Checked;
            label7.Enabled = radioButton5.Checked;
            textBox4.Text = "";
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (selectedPictureBoxes.Count > 0 && connectedClients.Count > 0)
            {
                #region MyRegion
                // Check Time System
                if (radioButton1.Checked || radioButton2.Checked || radioButton3.Checked)
                {
                    if (radioButton1.Checked == true && textBox3.Text == "")
                    {
                        MessageBox.Show("Please Write The Time (For Each Question) First"); return;
                    }
                    if (radioButton2.Checked == true && textBox2.Text == "")
                    {
                        MessageBox.Show("Please Write The Time (For All Question) First"); return;
                    }
                    coll = "Time<#>";
                    if (radioButton1.Checked)
                    {
                        coll = coll + "Each<#>" + textBox3.Text;
                    }
                    else if (radioButton2.Checked)
                    {
                        coll = coll + "All<#>" + textBox2.Text;
                    }
                    else if (radioButton3.Checked)
                    {
                        coll = coll + radioButton3.Text + "<#>0";
                    }
                }
                else
                {
                    MessageBox.Show("Please Choose The (Time) System First"); return;
                }

                // Check Points System
                if (radioButton6.Checked || radioButton5.Checked)
                {
                    if (radioButton6.Checked == true && textBox5.Text == "")
                    {
                        MessageBox.Show("Please Write The Points (For Each Correct Answer) First"); return;
                    }
                    if (radioButton5.Checked == true && textBox4.Text == "")
                    {
                        MessageBox.Show("Please Write The Points (For Each Wrong Answer) First"); return;
                    }
                    coll = coll + "<##>Points<#>";
                    if (radioButton6.Checked)
                    {
                        coll = coll + "Correct<#>" + textBox5.Text;
                    }
                    if (radioButton5.Checked && radioButton6.Checked)
                    {
                        coll = coll + "<#>Wrong<#>" + textBox4.Text;
                    }
                    else if (radioButton5.Checked)
                    {
                        coll = coll + "Wrong<#>" + textBox4.Text;
                    }
                }
                else
                {
                    MessageBox.Show("Please Choose The (Points) System First"); return;
                }

                // Check Answers System
                if (radioButton4.Checked || radioButton9.Checked)
                {
                    coll = coll + "<##>Answers<#>";
                    if (radioButton9.Checked)
                    {
                        coll = coll + "Mark";
                    }
                    else if (radioButton4.Checked)
                    {
                        coll = coll + "Hide";
                    }
                }
                else
                {
                    MessageBox.Show("Please Choose The (Answers) System First"); return;
                }

                // Check Questions System
                if (radioButton8.Checked || radioButton7.Checked)
                {
                    coll = coll + "<##>Questions<#>";
                    if (radioButton8.Checked)
                    {
                        coll = coll + "Random";
                    }
                    else if (radioButton7.Checked)
                    {
                        coll = coll + "Same";
                    }
                }
                else
                {
                    MessageBox.Show("Please Choose The (Questions) System First"); return;
                }
                #endregion

                SendSelectedPictureBoxesOverNetwork();
                update_ready_panel();
                tabControl1.SelectedIndex = 3;

                ////timer1.Start();
                //result = "";
                //List<string> imageNames = selectedPictureBoxes.Select(pb => pb.Name).ToList();
                //if (coll.Contains("Random", StringComparison.OrdinalIgnoreCase))
                //{
                //    // Create a new string with the image names in a random order
                //    Random random = new Random();
                //    List<string> randomImageNames = imageNames.OrderBy(_ => random.Next()).ToList();
                //    result = string.Join("<#>", randomImageNames.Distinct());
                //}
                //else if (coll.Contains("Same", StringComparison.OrdinalIgnoreCase))
                //{
                //    // Create a new string with the image names in sorted order
                //    List<string> sortedImageNames = imageNames.OrderBy(name => name).ToList();
                //    result = string.Join("<#>", sortedImageNames.Distinct());
                //}

            }
            else
            {

            }

            //string[] type1 = System.Text.RegularExpressions.Regex.Split(coll, "<##>");
            //timeeary = System.Text.RegularExpressions.Regex.Split(type1[0], "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();
            //Questionss = System.Text.RegularExpressions.Regex.Split(result, "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();

            //string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions", Questionss[0]);
            //pictureBox2.Image = Image.FromFile(imagePath + ".png");
            //pictureBox3.Image = Image.FromFile(imagePath + ".png");
            //pictureBox4.Image = Image.FromFile(imagePath + ".png");
            //pictureBox5.Image = Image.FromFile(imagePath + ".png");

            //// Convert seconds to TimeSpan
            //TimeSpan time = TimeSpan.FromSeconds(Convert.ToInt32(timeeary[2]));

            //// Format TimeSpan as string in hh:mm:ss format
            //string formattedTime = time.ToString(@"hh\:mm\:ss");

            //// Update the text of the Label
            //label15.Text = formattedTime;
        }

        private async Task SendSelectedPictureBoxesOverNetwork()
        {
            //byte[] sad = new byte[16];

            // Get the names of the selected images
            #region Random
            List<string> imageNames = selectedPictureBoxes.Select(pb => pb.Name).ToList();

            // Handle the conditions
            string result = "";
            if (coll.Contains("Random", StringComparison.OrdinalIgnoreCase))
            {
                // Create a new string with the image names in a random order
                Random random = new Random();
                List<string> randomImageNames = imageNames.OrderBy(_ => random.Next()).ToList();
                result = string.Join("<#>", randomImageNames.Distinct());
            }
            else if (coll.Contains("Same", StringComparison.OrdinalIgnoreCase))
            {
                // Create a new string with the image names in sorted order
                List<string> sortedImageNames = imageNames.OrderBy(name => name).ToList();
                result = string.Join("<#>", sortedImageNames.Distinct());
            }
            string QuestionsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions"); 
            #endregion

            if (connectedClients != null)
            {
                string connectedClients_Names = "";
                foreach (var clientInfo in connectedClients)
                {
                    connectedClients_Names = connectedClients_Names + clientInfo.Name + "<#>";
                }
                foreach (var clientInfo in connectedClients)
                {
                    //List<string> pictureBoxxName = new List<string>();
                    //List<string> imageDataStrings = new List<string>();
                    foreach (var pictureBox in selectedPictureBoxes)
                    {
                        //byte[] imageData;

                        //Convert PictureBox data to bytes
                        //string imageDataString = ConvertPictureBoxToBase64String(pictureBox, pictureBox.Name+".png");
                        string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions", pictureBox.Name + ".png");
                        await SendMessageToUserAsync("IMAGE:"+ pictureBox.Name + ".png", imagePath, clientInfo.Client);


                        //pictureBoxxName.Add(pictureBox.Name + ".PNG");
                        //imageDataStrings.Add(imageDataString);
                    }
                    //await SendMessageToUserAsync2(pictureBoxxName, imageDataStrings, clientInfo.Client);
                    await SendImageDataAsync("unique:" + connectedClients_Names, "", clientInfo.Client);
                    await SendImageDataAsync("Finished" + "<##>" + coll + "<##>" + result, "", clientInfo.Client);
                }
            }
        }
        private async Task SendMessageToUserAsync(string message, string imageData, TcpClient clientInfo)
        {
            try
            {
                Socket socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEndPoint = (IPEndPoint)clientInfo.Client.RemoteEndPoint;

                socketForClient.Connect(remoteEndPoint);
                byte[] fileNameData = Encoding.Default.GetBytes(message);
                socketForClient.Send(fileNameData);
                socketForClient.Shutdown(SocketShutdown.Both);
                socketForClient.Close();

                socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socketForClient.Connect(remoteEndPoint);
                socketForClient.SendFile(imageData);
                socketForClient.Shutdown(SocketShutdown.Both);
                socketForClient.Close();


            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message to user: {ex.Message}");
            }
        }
        private string ConvertPictureBoxToBase64String(PictureBox pictureBox,string pictureBoxxName)
        {
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions", pictureBoxxName);

            //byte[] imageArray = System.IO.File.ReadAllBytes(imagePath);
            //var base64String = Convert.ToBase64String(imageArray);
            //return base64String;
            using (Image image = Image.FromFile(imagePath))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);

                    // Validate the Base-64 string
                    //if (!Regex.IsMatch(base64String, @"^[A-Za-z0-9+/]*={0,2}$"))
                    //{
                    //    throw new ArgumentException("Invalid Base-64 string");
                    //}

                    //// Remove any potential URL-unsafe characters
                    //base64String = base64String.Replace('+', '-')
                    //                           .Replace('/', '_');

                    // Ensure correct padding (multiples of 4 characters)
                    //int padding = (base64String.Length % 4);
                    //if (padding > 0)
                    //{
                    //    base64String += new string('=', 4 - padding);
                    //}

                    //string converted = base64String.Replace('-', '+');
                    //converted = converted.Replace('_', '/');

                    //Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
                    //converted = regex.Replace(converted, string.Empty);
                    return base64String;
                }
            }

            //MemoryStream memoryStream = new MemoryStream();
            //// Save PictureBox image to MemoryStream
            //pictureBox.Image.Save(memoryStream, ImageFormat.Png);

            //// Convert MemoryStream to byte array
            //byte[] imageData = memoryStream.ToArray();

            //// Convert byte array to Base64 string
            //base64String = Convert.ToBase64String(imageData);

            ////try
            ////{
            ////    int padding = (base64String.Length % 4);
            ////    if (padding > 0)
            ////    {
            ////        base64String += new string('=', 4 - padding);
            ////    }
            ////}
            ////catch (Exception ex)
            ////{
            ////    MessageBox.Show("ConvertPictureBoxToBase64String " + ex);
            ////}
            //base64String = base64String.Replace('+', '-').Replace('/', '_');

            //// Remove non-base64 characters (e.g., line breaks) from the string
            //base64String = RemoveNonBase64Characters(base64String);

            //
        }
        private string RemoveNonBase64Characters(string input)
        {
            // Remove characters that are not part of the Base64 character set
            return new string(input.Where(c => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '=').ToArray());
        }
        private async Task SendImageDataAsync(string imageName, string imageDataString, TcpClient client)
        {
            // Send image data
            await SendMessageToUserAsync($"IMAGE:{imageName}", imageDataString, client);
        }
        private async Task SendMessageToUserAsync2(string message, string imageDataString, TcpClient userTcpClient)
        {
            try
            {
                if (userTcpClient != null && userTcpClient.Connected)
                {
                    NetworkStream stream = userTcpClient.GetStream();
                    StreamWriter writer = new StreamWriter(stream);
                    message = message + "<#>" + imageDataString;

                    // Send the Base64 string as a single line
                    await writer.WriteLineAsync($"{message}\n");
                    await writer.FlushAsync();
                }
                else
                {
                    //MessageBox.Show("The TcpClient is not connected.");
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error sending message to user: {ex.Message}");
            }
        }
        private async Task SendMessageToUserAsync(List<string> pictureBoxxName, List<string> imageDataStrings, TcpClient userTcpClient)
        {
            try
            {
                if (userTcpClient != null && userTcpClient.Connected)
                {
                    NetworkStream stream = userTcpClient.GetStream();
                    StreamWriter writer = new StreamWriter(stream);
                    string message = "";
                    for (int i = 0; i < pictureBoxxName.Count; i++)
                    {
                        message = $"IMAGE:" + pictureBoxxName[i] + "<#>" + imageDataStrings[i];
                        // Send the Base64 string as a single line
                        await writer.WriteLineAsync($"{message}\n");
                        await writer.FlushAsync();
                    }
                }
                else
                {
                    MessageBox.Show("The TcpClient is not connected.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message to user: {ex.Message}");
            }
        }
        #endregion

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
        int dsf = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            //// Parse the current time from label15
            //TimeSpan currentTime = TimeSpan.Parse(label15.Text);

            //// Add one second to the current time
            //currentTime = currentTime.Add(TimeSpan.FromSeconds(-1));

            //// Update count_Label with the new time
            //label15.Text = currentTime.ToString(@"hh\:mm\:ss");


            //if (timeeary[1].Contains("Each"))
            //{
            //    if (currentTime.ToString() == "00:00:00")
            //    {
            //        dsf++;
            //        // Convert seconds to TimeSpan
            //        TimeSpan time = TimeSpan.FromSeconds(Convert.ToInt32(timeeary[2]));

            //        // Format TimeSpan as string in hh:mm:ss format
            //        string formattedTime = time.ToString(@"hh\:mm\:ss");

            //        // Update the text of the Label
            //        label15.Text = formattedTime;
            //        if (dsf < Questionss.Length)
            //        {

            //            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions", Questionss[dsf]);
            //            pictureBox2.Image = Image.FromFile(imagePath + ".png");
            //            pictureBox3.Image = Image.FromFile(imagePath + ".png");
            //            pictureBox4.Image = Image.FromFile(imagePath + ".png");
            //            pictureBox5.Image = Image.FromFile(imagePath + ".png");

            //        }
            //        else { MessageBox.Show("Quiz Has Finished"); dsf = 0; timer1.Stop(); }
            //    }

            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Create an OpenFileDialog instance
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                // Set properties to allow multiple file selection and filter for image files
                Multiselect = true,
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All Files|*.*"
            };

            // Show the dialog and check if the user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Create a directory named "Questions" in the application directory
                string destinationFolder = Path.Combine(Application.StartupPath, "Questions");

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                // Iterate through selected files, load each image, and save as PNG
                foreach (string filePath in openFileDialog.FileNames)
                {
                    // Load the image
                    using (Image image = Image.FromFile(filePath))
                    {
                        // Get the file name without the path and change the extension to .png
                        string fileName = Path.GetFileNameWithoutExtension(filePath) + ".png";

                        // Build the destination path
                        string destinationPath = Path.Combine(destinationFolder, fileName);

                        // Save the image as PNG
                        image.Save(destinationPath, ImageFormat.Png);

                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Image = image;
                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox.Size = new Size(200, 200);
                        pictureBox.Click += pictureBox1_Click; // Add click event for selecting images

                        // Extract the filename without extension and set it as PictureBox name
                        string imageName = Path.GetFileNameWithoutExtension(fileName);
                        pictureBox.Name = imageName;

                        CheckBox checkBox = new CheckBox();
                        checkBox.Name = "selectionCheckbox";
                        checkBox.Visible = false; // Initially hide the checkbox
                        checkBox.Location = new Point(0, 0);
                        checkBox.Size = new Size(100, 30);
                        checkBox.Text = "Selected";
                        checkBox.ForeColor = Color.Red;
                        checkBox.Font = new Font("Arial", 12F, FontStyle.Bold);
                        pictureBox.Controls.Add(checkBox);
                        flowLayoutPanel1.Controls.Add(pictureBox);
                    }
                }
                try
                {
                    // MessageBox.Show("Images imported and saved");

                }
                catch { }
            }
        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (tabControl1.SelectedIndex == 2)
            {
                button10.Enabled = true;
                //if (selectedPictureBoxes.Count > 0 && connectedClients.Count > 0)
                //{
                //    button10.Enabled = true;
                //    button10.BackColor = Color.MediumSeaGreen;
                //}
                //else
                //{
                //    button10.ForeColor = Color.Black;
                //    button10.Enabled = false;
                //}
            }
            if (tabControl1.SelectedIndex == 3)
            {
                button10.Enabled = true;
                //if (selectedPictureBoxes.Count > 0 && connectedClients.Count > 0)
                //{
                //    button10.Enabled = true;
                //    button10.BackColor = Color.MediumSeaGreen;
                //}
                //else
                //{
                //    button10.ForeColor = Color.Black;
                //    button10.Enabled = false;
                //}
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //remove Q with all answer to next Q without any recording 
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //goto to next Qustion with recording all answer and it is time witout submition 
        }
        private void button13_Click(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (connectedClients != null)
            {
                foreach (var clientInfo in connectedClients)
                {
                    SendImageDataAsync("Start", "", clientInfo.Client);
                }
            }
        }

    }
}
