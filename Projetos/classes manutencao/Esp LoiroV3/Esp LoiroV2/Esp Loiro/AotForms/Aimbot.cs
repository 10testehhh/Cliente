using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace AotForms
{
    internal static class Aimbot
    {
        #region P/Invoke e Variáveis
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        // Adicionado para o atraso do Triggerbot
        private static Random random = new Random();
        #endregion

        internal static void Work()
        {
            while (true)
            {
                Thread.Sleep(1);
                ExecuteAimbotLogic();
                ExecuteTriggerbotLogic();
            }
        }

        #region Aimbot Logic (Melhorado)

        private static void ExecuteAimbotLogic()
        {
            if (!Config.AimbotAtivado || WinAPI.GetAsyncKeyState(Config.AimbotTecla) >= 0)
            {
                return;
            }

            Entity alvo = GetBestTarget();
            if (alvo != null)
            {
                AimAtTarget(alvo);
            }
        }

        /// <summary>
        /// NOVA VERSÃO: Encontra o melhor alvo com base na prioridade definida em Config.
        /// </summary>
        private static Entity GetBestTarget()
        {
            Entity melhorAlvo = null;
            float melhorMetrica = float.MaxValue;
            Vector2 centroDaTela = new Vector2(Core.Width / 2, Core.Height / 2);

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || !entity.IsKnown || !entity.isVisible || entity.IsTeam == Bool3.True || (Config.IgnoreKnocked && entity.IsKnocked))
                {
                    continue;
                }

                float metricaAtual = 0;

                switch (Config.PrioridadeDoAlvo)
                {
                    case Config.AimbotPrioridade.DistanciaDoJogador:
                        metricaAtual = entity.Distance;
                        break;

                    case Config.AimbotPrioridade.MenorVida:
                        metricaAtual = entity.Health;
                        break;

                    case Config.AimbotPrioridade.DistanciaDaMira:
                    default:
                        Vector2 headPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                        if (headPos.X <= 0 || headPos.Y <= 0) continue;
                        metricaAtual = Vector2.Distance(centroDaTela, headPos);
                        if (metricaAtual > Config.AimbotFov) continue; // Aplica o FOV apenas para esta prioridade
                        break;
                }

                if (metricaAtual < melhorMetrica)
                {
                    melhorMetrica = metricaAtual;
                    melhorAlvo = entity;
                }
            }
            return melhorAlvo;
        }

        /// <summary>
        /// NOVA VERSÃO: Mira no alvo com suavização dinâmica.
        /// </summary>
        private static void AimAtTarget(Entity alvo)
        {
            Vector3 targetWorldPos = alvo.Head; // Pode ser expandido com a seleção de osso que já fizemos
            Vector2 alvoScreenPos = W2S.WorldToScreen(Core.CameraMatrix, targetWorldPos, Core.Width, Core.Height);
            if (alvoScreenPos.X <= 0 || alvoScreenPos.Y <= 0) return;

            Vector2 centroDaTela = new Vector2(Core.Width / 2, Core.Height / 2);
            float deltaX = alvoScreenPos.X - centroDaTela.X;
            float deltaY = alvoScreenPos.Y - centroDaTela.Y;

            float suavizacao = Config.AimbotSuavizacao;

            // Lógica de Suavização Dinâmica
            if (Config.SuavizacaoDinamica)
            {
                // Aumenta a suavização para alvos mais distantes
                float fatorDistancia = Math.Max(1f, alvo.Distance / 50f); // A cada 50m, a suavização aumenta
                suavizacao *= fatorDistancia;
            }

            float moveX = deltaX / suavizacao;
            float moveY = deltaY / suavizacao;

            if (Math.Abs(moveX) > 0 || Math.Abs(moveY) > 0)
            {
                mouse_event(MOUSEEVENTF_MOVE, (int)moveX, (int)moveY, 0, 0);
            }
        }
        #endregion

        #region Triggerbot Logic (Melhorado)

        /// <summary>
        /// NOVA VERSÃO: Triggerbot com atraso para parecer mais humano.
        /// </summary>
        private static void ExecuteTriggerbotLogic()
        {
            if (!Config.TriggerbotAtivado)
            {
                return;
            }

            if (IsCrosshairOnEnemy())
            {
                // Adiciona um pequeno atraso aleatório para simular tempo de reação
                Thread.Sleep(random.Next(40, 90));
                Shoot();
            }
        }

        private static bool IsCrosshairOnEnemy()
        {
            Vector2 centroDaTela = new Vector2(Core.Width / 2, Core.Height / 2);
            const float triggerRadius = 15f;

            foreach (var entity in Core.Entities.Values)
            {
                if (entity.IsDead || !entity.IsKnown || !entity.isVisible || entity.IsTeam == Bool3.True || (Config.IgnoreKnocked && entity.IsKnocked))
                {
                    continue;
                }

                Vector2 headPos = W2S.WorldToScreen(Core.CameraMatrix, entity.Head, Core.Width, Core.Height);
                if (headPos.X > 0 && headPos.Y > 0 && Vector2.Distance(centroDaTela, headPos) < triggerRadius)
                {
                    return true;
                }

                Vector2 spinePos = W2S.WorldToScreen(Core.CameraMatrix, entity.Spine, Core.Width, Core.Height);
                if (spinePos.X > 0 && spinePos.Y > 0 && Vector2.Distance(centroDaTela, spinePos) < triggerRadius)
                {
                    return true;
                }
            }
            return false;
        }

        private static void Shoot()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        #endregion
    }
}
