using UnityEngine;
namespace Oxide.Plugins
{
    [Info("HeatVolume", "bmgjet", "1.0.1")]
    [Description("Burns a player when they contact heat_prefab")]
    public class HeatVolume : RustPlugin
    {
        public static float Damage = 3f; //Damage given each cycle.
        public static float Tempture = 3f;//Tempture increased by each cycle.
        private void OnPlayerRespawn(BasePlayer current)
        {
            AddHeatCheck(current);
        }

        void OnServerInitialized()
        {
            foreach (BasePlayer current in BasePlayer.activePlayerList)
            {
                AddHeatCheck(current);
            }
        }

        private void OnPlayerSleepEnded(BasePlayer current)
        {
            AddHeatCheck(current);
        }

        void AddHeatCheck(BasePlayer player)
        {
            if (player.GetComponent<Heat_Volume>() == null && !player.IsNpc)
            {
                player.gameObject.AddComponent<Heat_Volume>();
            }
        }

        void Unload()
        {
            var objects = GameObject.FindObjectsOfType(typeof(Heat_Volume));
            if (objects != null)
            {
                foreach (var gameObj in objects)
                {
                    GameObject.Destroy(gameObj);
                }
            }
        }

        private class Heat_Volume : FacepunchBehaviour
        {
            private BasePlayer _player;
            private Vector3 _pos;
            private void Awake()
            {
                _player = GetComponent<BasePlayer>();
                InvokeRepeating(Check, 1f, 0.4f);
            }

            private void Check()
            {
                if (_player == null || !_player.IsConnected)
                {
                    return;
                }
                _pos = _player.eyes.position;
                var hits = Physics.SphereCastAll(_pos, 0.01f, Vector3.one);
                foreach (var hit in hits)
                {
                    Collider bc = hit.GetCollider();
                    if (bc == null)
                        return;

                    if (bc.isTrigger && bc.name.Contains("heat_volume"))
                    {
                        if (!_player.IsAlive() || !_player.IsConnected)
                            return;

                        Effect.server.Run("assets/bundled/prefabs/fx/impacts/additive/fire.prefab", _pos);
                        _player.Hurt(Damage, Rust.DamageType.Heat, null, true);
                        _player.metabolism.temperature.value += Tempture;
                        return;
                    }
                }
            }
        }
    }
}