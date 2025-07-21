using System.Media;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing; // Adicionado para a classe Color

namespace AotForms
{
    internal static class Config
    {
        #region Configurações do Aimbot

        // Enum para a nova opção de prioridade de alvo
        public enum AimbotPrioridade
        {
            DistanciaDaMira,
            DistanciaDoJogador,
            MenorVida
        }

        /// <summary>
        /// Ativa ou desativa a funcionalidade do Aimbot.
        /// </summary>
        internal static bool AimbotAtivado = false;

        /// <summary>
        /// A tecla que o utilizador precisa de manter pressionada para o Aimbot funcionar.
        /// </summary>
        internal static Keys AimbotTecla = Keys.LButton;

        /// <summary>
        /// O campo de visão (Field of View) do Aimbot.
        /// </summary>
        internal static float AimbotFov = 90f;

        /// <summary>
        /// A suavidade base do movimento do Aimbot.
        /// </summary>
        internal static float AimbotSuavizacao = 5f;

        /// <summary>
        /// O osso do corpo do inimigo que o Aimbot irá mirar.
        /// </summary>
        internal static Bones AimbotAlvo = Bones.Head;

        /// <summary>
        /// Desenha um círculo no ecrã para visualizar o campo de visão (FOV) do Aimbot.
        /// </summary>
        internal static bool DesenharFovAimbot = true;

        /// <summary>
        /// Ativa o Triggerbot, que dispara automaticamente quando a mira está sobre um inimigo.
        /// </summary>
        internal static bool TriggerbotAtivado = false;

        // --- NOVAS CONFIGURAÇÕES ---

        /// <summary>
        /// Define como o Aimbot escolhe o melhor alvo.
        /// </summary>
        internal static AimbotPrioridade PrioridadeDoAlvo = AimbotPrioridade.DistanciaDaMira;

        /// <summary>
        /// Ativa a suavização dinâmica baseada na distância.
        /// </summary>
        internal static bool SuavizacaoDinamica = true;

        #endregion

        #region Configurações Existentes
        // Dentro da sua classe Config
        internal static bool EspEnabled = false; // Começa desligado por defeito
        internal static bool ESPHealth = false;
        internal static bool UseModernHealthBar = true; // <-- ADICIONE ESTA LINHA
        internal static Color ESPHeath = Color.White;
        internal static int expsize = 8;
        internal static int EspLineThickNess = 2;
        public static float GlowRadius = 16;
        public static float FeatherAmount = 3f;
        public static float GlowOpacity = 0.02f;
        internal static bool AimFov = false;
        internal static bool IgnoreKnocked = false;
        internal static bool NoRecoil = false;
        internal static bool NoCache = false;
        internal static bool aimIsVisible = true;
        internal static bool esptotalplyer = false;
        internal static bool FixEsp = false;
        internal static bool minimap = false;
        internal static bool ESPLine = false;
        internal static bool BoxGlow = false;
        internal static bool EspBottom = false;
        internal static bool EspUp = false;
        internal static Color ESPLineColor = Color.White;
        internal static bool ESPBox = false;
        internal static Color ESPBoxColor = Color.White;
        internal static bool ESPBox2 = false;
        internal static bool ESPDistance = false;
        internal static Color ESPFillBoxColor = Color.Red;
        internal static bool ESPName = false;
        internal static Color ESPNameColor = Color.White;
      //  internal static bool ESPHealth = false;
      //  internal static Color ESPHeath = Color.White;
        internal static bool ESPSkeleton = false;
        internal static Color ESPSkeletonColor = Color.White;
        internal static bool ESPFillBox = false;
        internal static bool ESPCorner = false;
        internal static bool ESPCornerColor = false;
        internal static bool ESPInfo = false;
        internal static bool ESPFove = false;
        internal static bool espbg = false;
        internal static bool Aimfovc = false;
        internal static Color Aimfovcolor = Color.White;
        internal static bool RGB = false;
        internal static bool espcfx = false;
        internal static bool sound = false;
        internal static int thread = 0;

        #endregion

        public static void Notif()
        {
            if (!sound)
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Client.clicksound.wav");
                if (stream != null)
                {
                    using (SoundPlayer player = new SoundPlayer(stream))
                    {
                        player.Play();
                    }
                }
            }
        }
    }
}
