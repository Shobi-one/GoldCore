using System.Collections.Generic;
using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Plays <see cref="EffectPreset"/>s from code: <c>Effects.Play(explosion, hit.point)</c>.
    /// Copies are pooled per preset, so firing a minigun does not allocate.
    /// </summary>
    public static class Effects
    {
        class Pool
        {
            public readonly Stack<EffectInstance> free = new Stack<EffectInstance>();
            public readonly List<EffectInstance> alive = new List<EffectInstance>();
        }

        static readonly Dictionary<EffectPreset, Pool> pools = new Dictionary<EffectPreset, Pool>();
        static Transform root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            pools.Clear();
            root = null;
        }

        /// <summary>
        /// Plays <paramref name="preset"/> at a point. Its forward axis is <paramref name="rotation"/>'s,
        /// so pass <c>Quaternion.LookRotation(hit.normal)</c> to spray out of a surface.
        /// With a <paramref name="parent"/> the effect follows it and takes its layer, which a weapon
        /// camera needs to draw a muzzle flash.
        /// </summary>
        public static EffectInstance Play(EffectPreset preset, Vector3 position, Quaternion rotation,
            Transform parent = null, float scale = 1f)
        {
            if (preset == null) return null;
            EffectInstance fx = Get(preset, scale);

            Transform t = fx.transform;
            t.SetParent(parent != null ? parent : Root(), false);
            t.SetPositionAndRotation(position, rotation);
            SetLayer(t, parent != null ? parent.gameObject.layer : 0);

            fx.Play();
            return fx;
        }

        public static EffectInstance Play(EffectPreset preset, Vector3 position) =>
            Play(preset, position, Quaternion.identity);

        /// <summary>Plays out of a surface, for impacts.</summary>
        public static EffectInstance Play(EffectPreset preset, RaycastHit hit) =>
            Play(preset, hit.point, Quaternion.LookRotation(hit.normal));

        /// <summary>Stops every copy of every effect, for example when loading a map.</summary>
        public static void KillAll()
        {
            foreach (Pool pool in pools.Values)
                foreach (EffectInstance fx in pool.alive.ToArray())
                    if (fx != null) fx.Kill();
        }

        static EffectInstance Get(EffectPreset preset, float scale)
        {
            if (!pools.TryGetValue(preset, out Pool pool))
                pools[preset] = pool = new Pool();
            pool.alive.RemoveAll(fx => fx == null);

            if (pool.alive.Count >= preset.maxInstances)
            {
                // Reuse the oldest copy rather than piling up effects. Kill hands it back to the free stack.
                EffectInstance oldest = pool.alive[0];
                oldest.Kill();
                pool.alive.Remove(oldest);
            }

            EffectInstance instance = null;
            while (instance == null && pool.free.Count > 0)
                instance = pool.free.Pop();

            if (instance == null)
            {
                var go = new GameObject(preset.name);
                instance = go.AddComponent<EffectInstance>();
                instance.Build(preset, scale);
            }
            else if (instance.builtVersion != preset.Version || !Mathf.Approximately(instance.builtScale, scale))
            {
                // The preset was edited (or a different scale was asked for): rebuild it.
                instance.Build(preset, scale);
            }

            pool.alive.Add(instance);
            return instance;
        }

        internal static void Release(EffectInstance fx)
        {
            if (fx.Preset == null || !pools.TryGetValue(fx.Preset, out Pool pool))
            {
                // Not from the pool, like the editor preview, which cleans up after itself.
                if (Application.isPlaying) Object.Destroy(fx.gameObject);
                return;
            }
            pool.alive.Remove(fx);
            if (!Application.isPlaying) return;

            // The effect may have followed something that is about to be destroyed.
            fx.transform.SetParent(Root(), false);
            fx.gameObject.SetActive(false);
            pool.free.Push(fx);
        }

        static Transform Root()
        {
            if (root != null) return root;
            var go = new GameObject("GoldCore Effects");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EffectRunner>();
            root = go.transform;
            return root;
        }

        static void SetLayer(Transform t, int layer)
        {
            t.gameObject.layer = layer;
            foreach (Transform child in t) SetLayer(child, layer);
        }

        internal static void TickAll(float dt)
        {
            foreach (Pool pool in pools.Values)
            {
                for (int i = pool.alive.Count - 1; i >= 0; i--)
                {
                    EffectInstance fx = pool.alive[i];
                    if (fx == null) pool.alive.RemoveAt(i);
                    else fx.Tick(dt);
                }
            }
        }
    }
}
