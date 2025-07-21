using ImGuiNET;
using System.Numerics;
using System.Drawing; // Adicionado para a classe Color
using AotForms;     // Adicionado para aceder ao Core e Config

namespace AotForms
{
    internal static class AimbotEsp
    {
        /// <summary>
        /// Desenha todos os elementos visuais relacionados ao Aimbot.
        /// </summary>
        public static void Render()
        {
            // Verifica se a opção de desenhar o FOV está ativa
            if (Config.AimbotAtivado && Config.DesenharFovAimbot)
            {
                uint whiteColor = ColorToUint32(Color.White);
                DrawSmoothCircle(Config.AimbotFov, whiteColor, 1.5f);
            }
        }

        /// <summary>
        /// Desenha um círculo suave no centro do ecrã.
        /// </summary>
        private static void DrawSmoothCircle(float radius, uint color, float thickness, int segments = 64)
        {
            var vList = ImGui.GetForegroundDrawList();
            var io = ImGui.GetIO();
            float centerX = io.DisplaySize.X / 2;
            float centerY = io.DisplaySize.Y / 2;
            vList.AddCircle(new Vector2(centerX, centerY), radius, color, segments, thickness);
        }

        /// <summary>
        /// Converte uma cor System.Drawing.Color para o formato uint do ImGui.
        /// </summary>
        private static uint ColorToUint32(Color color)
        {
            return ImGui.ColorConvertFloat4ToU32(new Vector4(
                (float)(color.R / 255.0),
                (float)(color.G / 255.0),
                (float)(color.B / 255.0),
                (float)(color.A / 255.0)));
        }
    }
}
