using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

// Namespace added to match the project structure and find 'Health' component
namespace Unity.Template.CompetitiveActionMultiplayer
{
    public class HealthBar : MonoBehaviour
    {
        // Assign your HUD.uxml here in the Inspector
        public UIDocument hudDocument;
        
        private VisualElement _healthFill;
        private World _clientWorld;

        // CHANGED: Use Awake instead of Start. Awake runs BEFORE OnEnable.
        void Awake()
        {
            if (hudDocument == null) hudDocument = GetComponent<UIDocument>();
        }

        void OnEnable()
        {
            // Verify we have the document
            if (hudDocument == null)
            {
                Debug.LogError("[HealthBar] Missing UIDocument! Assign it in Inspector or add component.");
                return;
            }

            // Wait for the UI to load
            var root = hudDocument.rootVisualElement;
            if (root == null) return;

            // Find the element by name
            _healthFill = root.Q<VisualElement>("HealthFill"); 

            if (_healthFill == null)
            {
                Debug.LogError("[HealthBar] Could not find VisualElement named 'HealthFill'. Check your UXML names.");
            }
            else
            {
                Debug.Log("[HealthBar] UI successfully connected.");
            }
        }

        void Update()
        {
            // If UI isn't ready, stop
            if (_healthFill == null) return;

            // 1. Find the Client World (where the player data lives)
            if (_clientWorld == null)
            {
                foreach (var world in World.All)
                {
                    if (world.IsClient() && !world.IsServer())
                    {
                        _clientWorld = world;
                        Debug.Log("[HealthBar] Found Client World!");
                        break;
                    }
                }
            }

            if (_clientWorld == null) return;

            // 2. Query for the Local Player's Health
            var entityManager = _clientWorld.EntityManager;
            
            // Look for an entity that has Health AND is owned by the local player
            var query = entityManager.CreateEntityQuery(
                typeof(Health), 
                typeof(GhostOwnerIsLocal) 
            );

            // Check if we found the player
            if (query.IsEmptyIgnoreFilter)
            {
                // This is normal while loading/spawning
                //Debug.LogWarning("[HealthBar] Waiting for Local Player Spawn...");
                return;
            }

            if (query.TryGetSingleton<Health>(out Health healthData))
            {
                // 3. Calculate Percentage
                float percent = healthData.CurrentHealth / healthData.MaxHealth * 100f;
                
                // 4. Update UI
                _healthFill.style.width = Length.Percent(percent);
            }
        }
    }
}