using Quiz_2.Formss;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using NetFwTypeLib;

namespace Quiz_2
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }
        private void btn_login_Click(object sender, EventArgs e)
        {
            btn_login.MouseEnter -= btn_login_MouseEnter;
            btn_login.MouseLeave -= btn_login_MouseLeave;

            this.Hide();
            var form2 = new Tchr();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var form2 = new copm_connect();
            this.Hide();
            form2.Closed += (s, args) => this.Close();
            form2.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button3_MouseHover(object sender, EventArgs e)
        {
            button3.ForeColor = Color.Red;
        }

        private void button3_Leave(object sender, EventArgs e)
        {
            button3.ForeColor = Color.Black;

        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            button3.ForeColor = System.Drawing.Color.Red;

        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            button3.ForeColor = System.Drawing.Color.Black;

        }

        private void btn_login_MouseEnter(object sender, EventArgs e)
        {
            btn_login.BackColor = Color.MediumSeaGreen;
        }

        private void btn_login_MouseLeave(object sender, EventArgs e)
        {
            btn_login.BackColor = Color.FromArgb(52, 152, 219);
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Specify the ports used by your quiz application
            string[] ports = { "12345", "12346" };

            foreach (var port in ports)
            {
                CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP);
                CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP);
            }
            // Create rules for each port for both TCP and UDP for private, public, and domain profiles
            //foreach (var port in ports)
            //{
            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE);
            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE);

            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC);
            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC);

            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN);
            //    CreateFirewallRule(port, (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_UDP, (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN);
            //}
        }
        static void CreateFirewallRule(string port, int protocol)
        {
            Type type = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            dynamic fwMgr = Activator.CreateInstance(type);
            dynamic localPolicy = fwMgr.LocalPolicy;
            dynamic firewallProfile = localPolicy.CurrentProfile;

            dynamic firewallRules = firewallProfile.GloballyOpenPorts;
            dynamic firewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWOpenPort"));

            firewallRule.Name = $"Quiz Application Rule ({port}, {protocol})";
            firewallRule.Port = Convert.ToInt32(port);
            firewallRule.Protocol = protocol;
            firewallRule.Enabled = true;

            firewallRules.Add(firewallRule);
        }
        static void CreateFirewallRule(string port, int protocol, int profileType)
        {
            Type type = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
            dynamic fwMgr = Activator.CreateInstance(type);
            dynamic localPolicy = fwMgr.LocalPolicy;
            dynamic firewallProfile = localPolicy.CurrentProfile;

            dynamic firewallRules = firewallProfile.GloballyOpenPorts;
            dynamic firewallRule = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWOpenPort"));

            firewallRule.Name = $"Quiz Application Rule ({port}, {protocol}, {GetProfileTypeName(profileType)})";
            firewallRule.Port = Convert.ToInt32(port);
            firewallRule.Protocol = protocol;
            firewallRule.Enabled = true;

            try
            {
                string combinedScopes = string.Join(", ", firewallRule.Scope);

                // Ensure that the profile type is valid for the Scope property
                if (firewallRule.Scope != profileType)
                {
                    throw new ArgumentException($"Invalid profile type: {profileType}");
                }
                else
                {
                    // Set the profile type for the rule
                    firewallRule.Scope = profileType;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in rules: {ex.Message}");
            }


            firewallRules.Add(firewallRule);
        }
        static string GetProfileTypeName(int profileType)
        {
            switch (profileType)
            {
                case (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PRIVATE:
                    return "Private";
                case (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_PUBLIC:
                    return "Public";
                case (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_DOMAIN:
                    return "Domain";
                default:
                    return "Unknown";
            }
        }
    }
}
