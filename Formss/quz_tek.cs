using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Quiz_2.Formss.quz_tek;
using Timer = System.Threading.Timer;

namespace Quiz_2.Formss
{
    public partial class quz_tek : Form
    {
        int Timee = 1;
        private string[] timeeary;
        private string[] Pointss;
        private string[] Answerss;
        private string[] Questionss;
        private int currentImageIndex = 0;
        int pngFileCount;
        Dictionary<string, ImageInfo> imageTimes = new Dictionary<string, ImageInfo>();
        public class ImageInfo
        {
            public string Image_Name { get; set; }
            public int Time { get; set; }//20sec
            public string Selected_Answer { get; set; }//-1 ABCD
            public int Time_to_answer { get; set; } //Time_to_select_answer
        }
        public quz_tek()
        {
            InitializeComponent();
        }
        private void quz_tek_Load(object sender, EventArgs e)
        {
            currentImageIndex = 0;
            string tempDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");

            if (Directory.Exists(tempDirPath))
            {
                pngFileCount = Directory.GetFiles(tempDirPath, "*.png").Length;
                if (pngFileCount > 9)
                {
                    lbl_qus_num.Text = "01/" + pngFileCount.ToString();
                }
                else
                {
                    lbl_qus_num.Text = "1/" + pngFileCount.ToString();
                }
                lbl_pont.Text = "0";

                //cong.confi = "Finished<##>Time<#>No<#>0<##>Points<#>Correct<#>1<#>Wrong<#>2<##>Answers<#>Mark<##>Questions<#>Random<##>Slide_0_A.PNG<#>Slide_1_B.PNG<#>Slide_2_C.PNG<#>Slide_3_D.PNG";
                string[] type1 = System.Text.RegularExpressions.Regex.Split(copm_connect.ControlID.confi, "<##>");
                timeeary = System.Text.RegularExpressions.Regex.Split(type1[1], "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();
                Pointss = System.Text.RegularExpressions.Regex.Split(type1[2], "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();
                Answerss = System.Text.RegularExpressions.Regex.Split(type1[3], "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();
                Questionss = System.Text.RegularExpressions.Regex.Split(type1[5], "<#>").Where(s => !string.IsNullOrEmpty(s)).ToArray();



                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", Questionss[currentImageIndex]);
                pictureBox2.Image = Image.FromFile(imagePath + ".png");

                #region Time Config
                imageTimes = new Dictionary<string, ImageInfo>();
                if (type1[1].Contains("Each"))
                {
                    for (int xsxcs = 0; xsxcs < Questionss.Length; xsxcs++)
                    {
                        if (!imageTimes.ContainsKey(Questionss[xsxcs]))
                        {
                            imageTimes.Add(Questionss[xsxcs], new ImageInfo { Image_Name = Questionss[xsxcs], Time = Convert.ToInt32(timeeary[2]), Selected_Answer = "-1" });
                        }
                        else
                        {
                            // Key exists, update the value
                            imageTimes[Questionss[xsxcs]].Time = Convert.ToInt32(timeeary[2]);
                        }
                    }
                }
                else
                {
                    for (int xsxcs = 0; xsxcs < Questionss.Length; xsxcs++)
                    {
                        if (!imageTimes.ContainsKey(Questionss[xsxcs]))
                        {
                            imageTimes.Add(Questionss[xsxcs], new ImageInfo { Image_Name = Questionss[xsxcs], Time = Convert.ToInt32(0), Selected_Answer = "-1" });
                        }
                        else
                        {
                            // Key exists, update the value
                            imageTimes[Questionss[xsxcs]].Time = 0;
                        }
                    }
                }
                if (type1[1].Contains("Each"))
                {
                    Timee = -1;
                    put_time(imageTimes[Questionss[currentImageIndex]].Time);
                }
                else if (type1[1].Contains("All"))
                {
                    Timee = -1;
                    put_time(Convert.ToInt32(timeeary[2]));
                }
                else
                {
                    Timee = 1;
                    put_time(Convert.ToInt32(timeeary[2]));
                }
                #endregion
                DialogResult result = MessageBox.Show("Quiz Will Start When you press OK", "Quiz Start", MessageBoxButtons.OK, MessageBoxIcon.Question);
                if (result == DialogResult.OK)
                {
                    timer1.Start();
                }
                if (currentImageIndex == 0)
                {
                    button1.Enabled = false;
                }
            }
        }
        private void put_time(int sec)
        {
            // Convert seconds to TimeSpan
            TimeSpan time = TimeSpan.FromSeconds(sec);

            // Format TimeSpan as string in hh:mm:ss format
            string formattedTime = time.ToString(@"hh\:mm\:ss");

            // Update the text of the Label
            count_Label.Text = formattedTime;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Parse the current time from count_Label
            TimeSpan currentTime = TimeSpan.Parse(count_Label.Text);

            // Add one second to the current time
            currentTime = currentTime.Add(TimeSpan.FromSeconds(Timee));
            if (timeeary[1].Contains("Each"))
            {
                imageTimes[Questionss[currentImageIndex]].Time = imageTimes[Questionss[currentImageIndex]].Time - 1;
            }
            else
            {
                imageTimes[Questionss[currentImageIndex]].Time = imageTimes[Questionss[currentImageIndex]].Time + 1;
            }

            if (button2.Enabled == false)
            {
                button2.Enabled = true;
                // Update time for the current image in the dictionary
                //imageTimes[Questionss[currentImageIndex]].Time_to_answer = imageTimes[Questionss[currentImageIndex]].Time_to_answer + 1;
            }
            else
            {
                // Update time for the current image in the dictionary
                imageTimes[Questionss[currentImageIndex]].Time_to_answer = imageTimes[Questionss[currentImageIndex]].Time_to_answer + 1;
            }

            // Update count_Label with the new time
            count_Label.Text = currentTime.ToString(@"hh\:mm\:ss");
            if (timeeary[1].Contains("Each") || timeeary[1].Contains("All"))
            {
                if (currentTime.ToString() == "00:00:00")
                {
                    //button4.Enabled = false;
                    //radioButton1.Checked = false;
                    //radioButton2.Checked = false;
                    //radioButton3.Checked = false;
                    //radioButton4.Checked = false;
                    //foreach (Control control in panel3.Controls)
                    //{
                    //    if (control is RadioButton radioButton && radioButton.Checked)
                    //    {
                    //        imageTimes[Questionss[currentImageIndex]].Selected_Answer = radioButton.Text;
                    //        break; // only want the first checked RadioButton
                    //    }
                    //}
                    button2.PerformClick();
                }
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
            radioButton3.Checked = false;
            radioButton4.Checked = false;
            foreach (Control control in panel3.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    imageTimes[Questionss[currentImageIndex]].Selected_Answer = radioButton.Text;
                    break; // only want the first checked RadioButton
                }
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            calculate_result();
            if (currentImageIndex < 0)
            {
                currentImageIndex = 0;
            }
            button1.Enabled = true;
            if (currentImageIndex < Questionss.Length && button2.Text != "Finish")
            {
                if (currentImageIndex == Questionss.Length - 2)
                {
                    button2.Text = "Finish";
                }

                currentImageIndex++;
                if (timeeary[1].Contains("Each"))
                {
                    // Check if the time for the current image is 0
                    if (imageTimes[Questionss[currentImageIndex]].Time <= 0)
                    {
                        // Skip to the next available image
                        button2.PerformClick(); return;
                        // SkipToNextAvailableImage();
                    }
                }

                // Use conditional formatting based on the length of pngFileCount
                string formattedNumber = (pngFileCount.ToString().Length == 2) ? (currentImageIndex + 1).ToString("D2") : (currentImageIndex + 1).ToString("D1");
                lbl_qus_num.Text = formattedNumber + "/" + pngFileCount.ToString();

                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", Questionss[currentImageIndex]);
                pictureBox2.Image = Image.FromFile(imagePath+".png");
                if (timeeary[1].Contains("Each"))
                {
                    Timee = -1;
                    put_time(imageTimes[Questionss[currentImageIndex]].Time);
                }
                get_selected_answer();
            }
            else
            {
                button2.Text = "Next";
                timer1.Stop();
                MessageBox.Show("Quiz Has Finished \nYou Scored: " + lbl_pont.Text + " Points"); //quz_tek_Load(sender, e);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            calculate_result();

            if (currentImageIndex == Questionss.Length)
            {
                currentImageIndex--;
            }
            if (currentImageIndex <= 0)
            {
                button1.Enabled = false;
            }
            if (button2.Text == "Finish")
            {
                button2.Text = "Next";
            }
            if (currentImageIndex > 0)
            {
                --currentImageIndex;

                if (timeeary[1].Contains("Each"))
                {
                    if (currentImageIndex >= 0)
                    {
                        if (imageTimes[Questionss[0]].Time == 0)
                        {
                            button2.PerformClick(); return;
                        }
                        if (imageTimes[Questionss[currentImageIndex]].Time == 0)
                        {
                            // Skip to the previos available image
                            button1.PerformClick(); return;
                        }
                    }
                    else
                    {
                        button2.PerformClick(); return;
                    }
                }

                // Use conditional formatting based on the length of pngFileCount
                string formattedNumber = (pngFileCount.ToString().Length == 2) ? (currentImageIndex + 1).ToString("D2") : (currentImageIndex + 1).ToString("D1");
                lbl_qus_num.Text = formattedNumber + "/" + pngFileCount.ToString();

                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", Questionss[currentImageIndex]);
                pictureBox2.Image = Image.FromFile(imagePath + ".png");
                if (timeeary[1].Contains("Each"))
                {
                    Timee = -1;
                    put_time(imageTimes[Questionss[currentImageIndex]].Time);
                }

                get_selected_answer();

                if (currentImageIndex == 0)
                {
                    button1.Enabled = false;
                }
            }
            else
            {
                button1.Enabled = false;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Hide();
            var form2 = new Tchr();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }
        private void calculate_result()
        {
            int correctCount = 0;
            int wrongCount = 0;
            foreach (Control control in panel3.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Checked)
                {
                    // The checked RadioButton is found
                    string fileName = imageTimes[Questionss[currentImageIndex]].Image_Name;

                    // Find the last occurrence of "_" in the string
                    int lastIndex = fileName.LastIndexOf('_');

                    // Check if "_" is found in the string
                    if (lastIndex != -1 && lastIndex < fileName.Length - 1)
                    {
                        // Get the character before "_"
                        char lastLetter = fileName[lastIndex + 1];
                        if (lastLetter.ToString() == radioButton.Text)
                        {
                            lbl_pont.Text = Convert.ToString(Convert.ToInt32(lbl_pont.Text) + Convert.ToInt32(Pointss[2]));
                        }
                        else if (Pointss.Length == 5)
                        {
                            lbl_pont.Text = Convert.ToString(Convert.ToInt32(lbl_pont.Text) - Convert.ToInt32(Pointss[4]));
                        }
                        imageTimes[Questionss[currentImageIndex]].Selected_Answer = radioButton.Text;
                    }
                    break; // only want the first checked RadioButton
                }
            }
            foreach (var kvp in imageTimes)
            {
                ImageInfo imageInfo = kvp.Value;

                // The checked RadioButton is found
                string fileName = imageInfo.Image_Name;

                // Find the last occurrence of "_" in the string
                int lastIndex = fileName.LastIndexOf('_');

                if (lastIndex != -1 && lastIndex < fileName.Length - 1)
                {
                    // Get the character before "_"
                    char lastLetter = fileName[lastIndex + 1];

                    // Compare with Selected_Answer
                    if (lastLetter.ToString() == imageInfo.Selected_Answer && imageInfo.Selected_Answer != "-1")
                    {
                        correctCount = correctCount + Convert.ToInt32(Pointss[2]);
                    }
                    else if (Pointss.Length == 5 && imageInfo.Selected_Answer != "-1")
                    {
                        wrongCount = wrongCount + Convert.ToInt32(Pointss[4]);
                    }
                }
            }
            lbl_pont.Text = Convert.ToString(correctCount - wrongCount);
            if (Answerss.Contains("Mark"))
            {
                button1.Visible = false;
            }
            else
            {
                label3.Visible = false;
                lbl_pont.Visible = false;
            }

            lbl_corr.Text = correctCount.ToString();
        }
        private void get_selected_answer()
        {
            //set the Selected Answer
            foreach (Control control in panel3.Controls)
            {
                if (control is RadioButton radioButton && radioButton.Text == imageTimes[Questionss[currentImageIndex]].Selected_Answer)
                {
                    radioButton.Checked = true;
                }
                //else
                //{
                //    ((RadioButton)control).Checked = false;
                //}
            }
        }
        private void radio_answer_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void quz_tek_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Stop();
        }
        public void send_sub()
        {

        }
    }
}
