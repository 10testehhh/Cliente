using Client;
using Guna.UI2.WinForms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AotForms
{
    public partial class MainMenu : Form
    {
        #region Declarações de Componentes (para compilação)



        #endregion

        private bool isProcessRunning = false;
        private IntPtr mainHandle;

        public MainMenu(IntPtr handle)
        {
            InitializeComponent();
            this.autorefresh.Tick += new System.EventHandler(this.autorefresh_Tick);
            mainHandle = handle;
        }

        public MainMenu()
        {
            InitializeComponent();
            this.autorefresh.Tick += new System.EventHandler(this.autorefresh_Tick);
        }

        #region Funções Solicitadas

        private async void guna2CustomCheckBox3_Click_1(object sender, EventArgs e)
        {
            Config.Notif();
            if (guna2CustomCheckBox3.Checked)
            {
                try
                {
                    if (!isProcessRunning)
                    {
                        isProcessRunning = true;
                        UpdateStatus("Launching VIP ModMenu, please wait...", Color.Yellow);

                        var processes = Process.GetProcessesByName("HD-Player");
                        if (processes.Length == 0)
                        {
                            UpdateStatus("HD-Player process not found.", Color.Red);
                            isProcessRunning = false; // Reset flag on failure
                            return;
                        }

                        UpdateStatus("Checking emulator components...", Color.Yellow);
                        await Task.Delay(500);

                        var process = processes[0];
                        var mainModulePath = Path.GetDirectoryName(process.MainModule.FileName);
                        var adbPath = Path.Combine(mainModulePath, "HD-Adb.exe");

                        if (!File.Exists(adbPath))
                        {
                            UpdateStatus("ADB not found! Ensure emulator is properly installed.", Color.Red);
                            isProcessRunning = false; // Reset flag on failure
                            return;
                        }

                        var adb = new Adb(adbPath);
                        await adb.Kill();
                        UpdateStatus("Restarting ADB service...", Color.Yellow);

                        var started = await adb.Start();
                        if (!started)
                        {
                            UpdateStatus("Failed to start ADB!", Color.Red);
                            isProcessRunning = false; // Reset flag on failure
                            return;
                        }

                        UpdateStatus("Checking game package...", Color.Yellow);

                        string pkg = "com.dts.freefireth";
                        string lib = "libil2cpp.so";
                        bool isFreeFireMax = false;
                        if (isFreeFireMax)
                        {
                            pkg = "com.dts.freefiremax";
                        }
                        await Task.Delay(500);

                        UpdateStatus("Locating game module...", Color.Yellow);
                        var moduleAddr = await adb.FindModule(pkg, lib);
                        if (moduleAddr == 0)
                        {
                            UpdateStatus("Module not found!", Color.Red);
                            isProcessRunning = false; // Reset flag on failure
                            return;
                        }

                        await Task.Delay(500);
                        UpdateStatus($"Initialization Successful! Address: {moduleAddr.ToString("X")}", Color.Yellow);
                        await Task.Delay(500);

                        Offsets.Il2Cpp = moduleAddr;
                        Core.Handle = FindRenderWindow(mainHandle);

                        UpdateStatus("Initializing ESP and Aimbot...", Color.Yellow);
                        var esp = new ESP();
                        await esp.Start();

                        await Task.Delay(500);
                        UpdateStatus("Activating Aimbot features...", Color.Yellow);
                        new Thread(Data.Work) { IsBackground = true }.Start();
                        // ADICIONE ESTA NOVA LINHA AQUI:
                        // Ela inicia o Aimbot para que ele comece a funcionar em segundo plano
                        new Thread(Aimbot.Work) { IsBackground = true }.Start();

                        UpdateStatus("Setup complete! Have fun!", Color.Lime);
                    }
                    else
                    {
                        UpdateStatus("ModMenu Already Running....", Color.Blue);
                    }
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Error: {ex.Message}", Color.Red);
                    isProcessRunning = false; // Reset flag on error
                }
            }
        }

        private void guna2CustomCheckBox17_Click(object sender, EventArgs e)
        {
            Config.ESPLine = guna2CustomCheckBox17.Checked;
            Config.EspUp = guna2CustomCheckBox17.Checked;
            status.Text = guna2CustomCheckBox17.Checked ? "ESP LINE ENABLED" : "ESP LINE DISABLED";
        }

        private void guna2CustomCheckBox16_Click(object sender, EventArgs e)
        {
            bool isChecked = guna2CustomCheckBox16.Checked;
            Config.ESPBox = isChecked;
            status.Text = isChecked ? "ESP BOX ENABLED" : "ESP BOX DISABLED";

        }

        private void guna2CustomCheckBox15_Click(object sender, EventArgs e)
        {
            bool isChecked = guna2CustomCheckBox15.Checked;
            Config.ESPName = isChecked;
            Config.ESPDistance = isChecked;
            Config.ESPHealth = isChecked;
            status.Text = isChecked ? "ESP INFO ENABLED" : "ESP INFO DISABLED";

        }

        private void guna2CustomCheckBox14_Click(object sender, EventArgs e)
        {
            Config.ESPSkeleton = guna2CustomCheckBox14.Checked;
            status.Text = guna2CustomCheckBox14.Checked ? "ESP SKELETON ENABLED" : "ESP SKELETON DISABLED";
        }

        private void guna2CustomCheckBox13_Click(object sender, EventArgs e)
        {
            Config.minimap = guna2CustomCheckBox13.Checked;
            status.Text = guna2CustomCheckBox13.Checked ? "ESP MINIMAP ENABLED" : "ESP MINIMAP DISABLED";
        }

        #endregion

        #region Métodos de Dependência
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        static IntPtr FindRenderWindow(IntPtr parent)
        {
            IntPtr renderWindow = IntPtr.Zero;
            EnumChildWindows(parent, (hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string windowName = sb.ToString();
                if (!string.IsNullOrEmpty(windowName) && windowName != "HD-Player")
                {
                    renderWindow = hWnd;
                }
                return true;
            }, IntPtr.Zero);
            return renderWindow;
        }

        private void UpdateStatus(string message, Color color)
        {
            if (status.InvokeRequired)
            {
                status.Invoke(new Action(() =>
                {
                    status.Text = message;
                    status.ForeColor = color;
                }));
            }
            else
            {
                status.Text = message;
                status.ForeColor = color;
            }
        }
        #endregion

        #region Auto-Refresh
        private void autorefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                // CORREÇÃO: As linhas foram descomentadas para que a função seja executada.
                InternalMemory.Cache = new();
                Core.Entities = new();
                Console.WriteLine("Cache e Entidades atualizados às: " + DateTime.Now.ToLongTimeString());
            }
            catch (Exception ex)
            {
                autorefresh.Stop();
                MessageBox.Show("Ocorreu um erro durante o auto-refresh: " + ex.Message);
            }
        }

        private void chkAutoRefresh_CheckedChanged(object sender, EventArgs e)
        {
            // Tenta converter o 'sender' para um Guna2CustomCheckBox.
            // Adapte para o controle que você está usando (ex: CheckBox, Guna2ToggleSwitch).
            var chk = sender as Guna2CustomCheckBox;
            if (chk == null) return;

            if (chk.Checked)
            {
                autorefresh.Interval = 3000;
                autorefresh.Start();
                UpdateStatus("Auto-refresh ativado.", Color.Green);
            }
            else
            {
                autorefresh.Stop();
                UpdateStatus("Auto-refresh desativado.", Color.Red);
            }
            // CORREÇÃO: A diretiva #endregion foi removida daqui.
        }
        #endregion

        #region Handlers de Eventos Vazios (para compilação)
        private void guna2Panel51_Paint(object sender, PaintEventArgs e) { }
        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
        private void rgb_Click(object sender, EventArgs e) { }
        private void Form2_Load(object sender, EventArgs e) { }
        private void label38_Click(object sender, EventArgs e) { }
        #endregion

        private void guna2CustomCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            // Pega o estado do seu checkbox (marcado ou desmarcado)
            var checkbox = sender as Guna.UI2.WinForms.Guna2CustomCheckBox;

            // Atribui esse estado à nossa variável de configuração do aimbot.
            Config.AimbotAtivado = checkbox.Checked;

            // Opcional: Dá um feedback visual ao utilizador sobre o estado do aimbot.
            if (Config.AimbotAtivado)
            {
                UpdateStatus("Aimbot Ativado", Color.Green);
            }
            else
            {
                UpdateStatus("Aimbot Desativado", Color.Red);
            }
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            // Pega o valor atual da TrackBar
            var trackBar = sender as Guna.UI2.WinForms.Guna2TrackBar;

            // Atualiza a variável de configuração do FOV com o novo valor
            Config.AimbotFov = trackBar.Value;
            // ADICIONE ESTA LINHA:
            // Ela atualiza o texto do Label com o valor do FOV.
            labelFovValor.Text = Config.AimbotFov.ToString();
            // Opcional: Mostra o valor atual num Label para o utilizador ver
            // labelFovValue.Text = Config.AimbotFov.ToString();
        }

        private void guna2TrackBar2_Scroll(object sender, ScrollEventArgs e)
        {
            // Pega o valor atual da TrackBar
            var trackBar = sender as Guna.UI2.WinForms.Guna2TrackBar;

            // Atualiza a variável de configuração da suavização com o novo valor
            Config.AimbotSuavizacao = trackBar.Value;
            // ADICIONE ESTA LINHA:
            // Ela atualiza o texto do Label com o valor da Suavidade.
            labelSuavidadeValor.Text = Config.AimbotSuavizacao.ToString();
            // Opcional: Se tiver um Label para mostrar o valor, atualize-o aqui
            // labelSuavidadeValor.Text = Config.AimbotSuavizacao.ToString();
        }

        private void guna2CustomCheckBox2_Click(object sender, EventArgs e)
        {

        }



        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void BoneSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Pega o item selecionado no ComboBox
            var comboBox = sender as Guna.UI2.WinForms.Guna2ComboBox;
            string selectedBone = comboBox.SelectedItem.ToString();

            // Atualiza a configuração do Aimbot com base na escolha do utilizador
            switch (selectedBone)
            {
                case "Chest":
                    Config.AimbotAlvo = Bones.Spine; // "Chest" (Peito) vai mirar na "Spine" (Coluna)
                    break;
                case "Hip":
                    Config.AimbotAlvo = Bones.Hip;
                    break;
                case "Head":
                default:
                    Config.AimbotAlvo = Bones.Head;
                    break;
            }

            // Opcional: Mostra uma mensagem de status para o utilizador
            UpdateStatus($"Alvo do Aimbot definido para: {selectedBone}", Color.Blue);
        }

        private void guna2CustomCheckBox4_CheckedChanged(object sender, EventArgs e)
        {
            // Pega o estado do seu checkbox (marcado ou desmarcado)
            var checkbox = sender as Guna.UI2.WinForms.Guna2CustomCheckBox;

            // Atualiza a variável de configuração do Triggerbot
            Config.TriggerbotAtivado = checkbox.Checked;

            // Opcional: Dá um feedback visual ao utilizador
            if (Config.TriggerbotAtivado)
            {
                UpdateStatus("Triggerbot Ativado", Color.Green);
            }
            else
            {
                UpdateStatus("Triggerbot Desativado", Color.Red);
            }

        }

        private void guna2CustomCheckBox5_CheckedChanged(object sender, EventArgs e)
        {
            // Pega o estado do seu checkbox (marcado ou desmarcado)
            var checkbox = sender as Guna.UI2.WinForms.Guna2CustomCheckBox;

            // Atualiza a variável de configuração da suavização dinâmica
            Config.SuavizacaoDinamica = checkbox.Checked;

            // Opcional: Dá um feedback visual ao utilizador
            if (Config.SuavizacaoDinamica)
            {
                UpdateStatus("Suavização Dinâmica Ativada", Color.Green);
            }
            else
            {
                UpdateStatus("Suavização Dinâmica Desativada", Color.Red);
            }
        }

        private void guna2CustomCheckBox6_CheckedChanged(object sender, EventArgs e)
        {
            // Pega o estado do seu checkbox (marcado ou desmarcado)
            var checkbox = sender as Guna.UI2.WinForms.Guna2CustomCheckBox;

            // Atualiza a variável de configuração do ESP com o estado do botão
            Config.EspEnabled = checkbox.Checked;

            // Opcional: Dá um feedback visual ao utilizador
            if (Config.EspEnabled)
            {
                UpdateStatus("ESP Ativado", Color.Green);
            }
            else
            {
                UpdateStatus("ESP Desativado", Color.Red);
            }

        }

       
    }
}
