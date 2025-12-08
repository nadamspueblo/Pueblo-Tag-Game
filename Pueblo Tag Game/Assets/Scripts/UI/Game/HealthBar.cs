using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Template.CompetitiveActionMultiplayer
{
    public class HealthBar : MonoBehaviour
    {
        public UIDocument hudDocument;
        
        [Header("Debug Settings")]
        public bool debugMode = false; 

        private VisualElement _healthFill;
        private World _clientWorld;

        void Awake()
        {
            // Safety Check 1: Ensure we have the component
            if (hudDocument == null) hudDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            if (hudDocument == null) return;

            var root = hudDocument.rootVisualElement;
            if (root == null) return;

            _healthFill = root.Q<VisualElement>("HealthFill"); 
            
            // Safety Check 2: Reset world reference on enable to force re-finding it
            _clientWorld = null;
        }

        void Update()
        {
            // Safety Check 3: UI Existence
            if (_healthFill == null) return;

            // --- DEBUG MODE ---
            if (debugMode)
            {
                float pingPong = Mathf.PingPong(Time.time * 50f, 100f);
                _healthFill.style.width = Length.Percent(pingPong);
                return;
            }

            // Safety Check 4: Find World (Robust)
            if (_clientWorld == null || !_clientWorld.IsCreated)
            {
                // Reset to null if the world was destroyed (e.g. scene change)
                _clientWorld = null;

                foreach (var world in World.All)
                {
                    if (world.IsClient() && !world.IsServer())
                    {
                        _clientWorld = world;
                        break;
                    }
                }
            }

            // If we still don't have a world, stop.
            if (_clientWorld == null || !_clientWorld.IsCreated) return;

            // Safety Check 5: EntityManager Validity
            var entityManager = _clientWorld.EntityManager;
            
            // Query ECS
            var query = entityManager.CreateEntityQuery(
                typeof(Health), 
                typeof(GhostOwnerIsLocal) 
            );

            if (query.IsEmptyIgnoreFilter) return;

            // Safety Check 6: Component Data Validity
            // Sometimes the entity exists but the component data is garbage/defaults
            if (query.TryGetSingleton<Health>(out Health healthData))
            {
                // Prevent DivideByZero
                if (healthData.MaxHealth <= 0) return;

                float percent = Mathf.Clamp(healthData.CurrentHealth / healthData.MaxHealth * 100f, 0f, 100f);
                _healthFill.style.width = Length.Percent(percent);
            }
        }
    }
}