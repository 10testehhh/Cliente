using System.Numerics;
using System.Collections.Generic; // Adicionado para HashSet

namespace AotForms
{
    internal static class Data
    {
        internal static void Work()
        {
            while (true)
            {
                Core.HaveMatrix = false;

                var rBaseGameFacade = InternalMemory.Read<uint>(Offsets.Il2Cpp + Offsets.InitBase, out var baseGameFacade);
                if (!rBaseGameFacade || baseGameFacade == 0) { ResetCache(); continue; }

                var rGameFacade = InternalMemory.Read<uint>(baseGameFacade, out var gameFacade);
                if (!rGameFacade || gameFacade == 0) { ResetCache(); continue; }

                var rStaticGameFacade = InternalMemory.Read<uint>(gameFacade + Offsets.StaticClass, out var staticGameFacade);
                if (!rStaticGameFacade || staticGameFacade == 0) { ResetCache(); continue; }

                var rCurrentGame = InternalMemory.Read<uint>(staticGameFacade, out var currentGame);
                if (!rCurrentGame || currentGame == 0) { ResetCache(); continue; }

                var rCurrentMatch = InternalMemory.Read<uint>(currentGame + Offsets.CurrentMatch, out var currentMatch);
                if (!rCurrentMatch || currentMatch == 0) { ResetCache(); continue; }

                var rLocalPlayer = InternalMemory.Read<uint>(currentMatch + Offsets.LocalPlayer, out var localPlayer);
                if (!rLocalPlayer || localPlayer == 0) continue;

                Core.LocalPlayer = localPlayer;

                var rMainTransform = InternalMemory.Read<uint>(localPlayer + Offsets.MainCameraTransform, out var mainTransform);
                if (!rMainTransform || mainTransform == 0) continue;

                if (Transform.GetPosition(mainTransform, out var mainPos))
                {
                    Core.LocalMainCamera = mainPos;
                }

                var rFollowCamera = InternalMemory.Read<uint>(localPlayer + Offsets.FollowCamera, out var followCamera);
                if (!rFollowCamera || followCamera == 0) continue;

                var rCamera = InternalMemory.Read<uint>(followCamera + Offsets.Camera, out var camera);
                if (!rCamera || camera == 0) continue;

                var rCameraBase = InternalMemory.Read<uint>(camera + 0x8, out var cameraBase);
                if (!rCameraBase || cameraBase == 0) continue;

                Core.HaveMatrix = true;
                if (!InternalMemory.Read<Matrix4x4>(cameraBase + Offsets.ViewMatrix, out Core.CameraMatrix)) continue;

                var rEntityDictionary = InternalMemory.Read<uint>(currentGame + Offsets.DictionaryEntities, out var entityDictionary);
                if (!rEntityDictionary || entityDictionary == 0) { ResetCache(); continue; }

                var rEntities = InternalMemory.Read<uint>(entityDictionary + 0x14, out var entities);
                if (!rEntities || entities == 0) { ResetCache(); continue; }

                if (Config.NoRecoil)
                {
                    if (InternalMemory.Read<uint>(localPlayer + Offsets.Weapon, out var weapon) && weapon != 0)
                    {
                        if (InternalMemory.Read<uint>(weapon + Offsets.WeaponData, out var weaponData) && weaponData != 0)
                        {
                            if (InternalMemory.Read<float>(weaponData + Offsets.WeaponRecoil, out var recoil) && recoil != 0)
                            {
                                InternalMemory.Write(weaponData + Offsets.WeaponRecoil, 0f);
                            }
                        }
                    }
                }

                entities = entities + 0x10;
                if (!InternalMemory.Read<uint>(entityDictionary + 0x18, out var entitiesCount) || entitiesCount < 1) continue;

                // --- NOVO SISTEMA DE LIMPEZA INTELIGENTE ---
                // 1. Tira uma "foto" das entidades que existem atualmente na nossa lista.
                var knownEntities = new HashSet<long>(Core.Entities.Keys);
                // --- FIM DO NOVO SISTEMA ---

                for (int i = 0; i < entitiesCount; i++)
                {
                    if (!InternalMemory.Read<uint>((ulong)(i * 0x4 + entities), out var entity) || entity == 0 || entity == localPlayer) continue;

                    // --- NOVO SISTEMA DE LIMPEZA INTELIGENTE ---
                    // 2. Se a entidade ainda existe no jogo, removemo-la da nossa "foto".
                    knownEntities.Remove(entity);
                    // --- FIM DO NOVO SISTEMA ---

                    if (Core.Entities.TryGetValue(entity, out var player))
                    {
                        // ... (toda a sua lógica de atualização do 'player' continua aqui) ...
                        if (player.IsTeam == Bool3.True) continue;

                        if (player.IsTeam == Bool3.Unknown)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.AvatarManager, out var avatarManager) && avatarManager != 0)
                            {
                                if (InternalMemory.Read<uint>(avatarManager + Offsets.Avatar, out var avatar) && avatar != 0)
                                {
                                    if (InternalMemory.Read<bool>(avatar + Offsets.Avatar_IsVisible, out var isVisible) && isVisible)
                                    {
                                        player.isVisible = isVisible;
                                        if (InternalMemory.Read<uint>(avatar + Offsets.Avatar_Data, out var avatarData) && avatarData != 0)
                                        {
                                            if (InternalMemory.Read<bool>(avatarData + Offsets.Avatar_Data_IsTeam, out var isTeam))
                                            {
                                                player.IsTeam = isTeam ? Bool3.True : Bool3.False;
                                                if (!isTeam) player.IsKnown = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!player.IsKnown) continue;

                        if (InternalMemory.Read<bool>(entity + Offsets.Player_IsDead, out var isDead)) player.IsDead = isDead;

                        if (Config.IgnoreKnocked)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.Player_ShadowBase, out var shadowBase) && shadowBase != 0)
                            {
                                if (InternalMemory.Read<int>(shadowBase + Offsets.XPose, out var xpose))
                                {
                                    player.IsKnocked = xpose == 8;
                                }
                            }
                        }

                        if (Config.ESPName)
                        {
                            if (InternalMemory.Read<uint>(entity + Offsets.Player_Name, out var nameAddr) && nameAddr != 0)
                            {
                                if (InternalMemory.Read<int>(nameAddr + 0x8, out var nameLen) && nameLen > 0)
                                {
                                    var name = InternalMemory.ReadString(nameAddr + 0xC, nameLen);
                                    if (!string.IsNullOrEmpty(name)) player.Name = name;
                                }
                            }
                        }

                        if (InternalMemory.Read<uint>(entity + Offsets.Player_Data, out var dataPool) && dataPool != 0)
                        {
                            if (InternalMemory.Read<uint>(dataPool + 0x8, out var poolObj) && poolObj != 0)
                            {
                                if (InternalMemory.Read<uint>(poolObj + 0x10, out var pool) && pool != 0)
                                {
                                    if (InternalMemory.Read<short>(pool + 0x10, out var health)) player.Health = health;
                                }
                            }
                        }

                        var boneOffsets = new[]
                        {
                            Bones.Head, Bones.LeftWrist, Bones.Spine, Bones.Hip, Bones.Root,
                            Bones.RightCalf, Bones.LeftCalf, Bones.RightFoot, Bones.LeftFoot,
                            Bones.RightWrist, Bones.LeftHand, Bones.LeftSholder, Bones.RightSholder,
                            Bones.RightWristJoint, Bones.LeftWristJoint, Bones.LeftElbow, Bones.RightElbow
                        };

                        foreach (var offset in boneOffsets)
                        {
                            if (InternalMemory.Read<uint>(entity + (uint)offset, out var bone) && bone != 0)
                            {
                                if (Transform.GetNodePosition(bone, out var boneTransform))
                                {
                                    switch (offset)
                                    {
                                        case Bones.Head: player.Head = boneTransform; break;
                                        case Bones.LeftWrist: player.LeftWrist = boneTransform; break;
                                        case Bones.Spine: player.Spine = boneTransform; break;
                                        case Bones.Hip: player.Hip = boneTransform; break;
                                        case Bones.Root: player.Root = boneTransform; break;
                                        case Bones.RightCalf: player.RightCalf = boneTransform; break;
                                        case Bones.LeftCalf: player.LeftCalf = boneTransform; break;
                                        case Bones.RightFoot: player.RightFoot = boneTransform; break;
                                        case Bones.LeftFoot: player.LeftFoot = boneTransform; break;
                                        case Bones.RightWrist: player.RightWrist = boneTransform; break;
                                        case Bones.LeftHand: player.LeftHand = boneTransform; break;
                                        case Bones.LeftSholder: player.LeftSholder = boneTransform; break;
                                        case Bones.RightSholder: player.RightSholder = boneTransform; break;
                                        case Bones.RightWristJoint: player.RightWristJoint = boneTransform; break;
                                        case Bones.LeftWristJoint: player.LeftWristJoint = boneTransform; break;
                                        case Bones.RightElbow: player.RightElbow = boneTransform; break;
                                        case Bones.LeftElbow: player.LeftElbow = boneTransform; break;
                                    }
                                }
                            }
                        }
                        player.Distance = Vector3.Distance(Core.LocalMainCamera, player.Head);
                    }
                    else
                    {
                        Core.Entities[entity] = new Entity
                        {
                            IsTeam = Bool3.Unknown,
                            IsKnown = false,
                            IsDead = false,
                            Health = 200,
                            IsKnocked = false,
                            Head = Vector3.Zero,
                            LeftWrist = Vector3.Zero,
                            Spine = Vector3.Zero,
                            Root = Vector3.Zero,
                            Hip = Vector3.Zero,
                            RightCalf = Vector3.Zero,
                            LeftCalf = Vector3.Zero,
                            RightFoot = Vector3.Zero,
                            LeftFoot = Vector3.Zero,
                            RightWrist = Vector3.Zero,
                            LeftHand = Vector3.Zero,
                            RightSholder = Vector3.Zero,
                            RightWristJoint = Vector3.Zero,
                            LeftWristJoint = Vector3.Zero,
                            RightElbow = Vector3.Zero,
                            LeftElbow = Vector3.Zero,
                            Name = ""
                        };
                    }
                }

                // --- NOVO SISTEMA DE LIMPEZA INTELIGENTE ---
                // 3. Remove da nossa lista principal todas as entidades que estavam na "foto"
                // mas que não foram encontradas no jogo desta vez (os "fantasmas").
                foreach (var oldEntityKey in knownEntities)
                {
                    Core.Entities.TryRemove(oldEntityKey, out _);
                }
                // --- FIM DO NOVO SISTEMA ---
            }
        }

        public static void ResetCache()
        {
            Core.Entities = new();
            InternalMemory.Cache = new();
        }
    }
}
