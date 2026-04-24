using UnityEngine;
using UnityEditor;
using LastMachine.Arandia;

namespace LastMachine.Arandia.Editor
{
    public class SceneSetupWizard_Arandia : EditorWindow
    {
        [MenuItem("Tools/Last Machine/Auto Setup Scene")]
        public static void SetupScene()
        {
            // 1. Tags & Layers
            CreateTagsAndLayers();

            // 2. Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(5, 1, 5);

            // 3. Camera
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0, 30, 0);
                mainCam.transform.rotation = Quaternion.Euler(90, 0, 0);
                mainCam.fieldOfView = 60;
            }

            // 4. Models
            GameObject baseModel = LoadFBX("baseTorretaSano");
            GameObject motorModel = LoadFBX("motor");
            GameObject radarModel = LoadFBX("radar");
            GameObject canonModel = LoadFBX("Torreta");

            if (baseModel == null || motorModel == null || radarModel == null || canonModel == null)
            {
                Debug.LogError("[Auto Setup] Error: No se encontraron los modelos. Asegurate que estan en 02_Model/TorretaArandia/");
                return;
            }

            // 5. Proyectil
            GameObject projPrefab = CreateProjectilePrefab();

            // 6. Torretas
            TurretController_Arandia tNorte = CreateTurret("Torreta_Norte", new Vector3(0, 0, 20), TurretDirection.Norte, baseModel, motorModel, radarModel, canonModel, projPrefab);
            TurretController_Arandia tSur = CreateTurret("Torreta_Sur", new Vector3(0, 0, -20), TurretDirection.Sur, baseModel, motorModel, radarModel, canonModel, projPrefab);
            TurretController_Arandia tEste = CreateTurret("Torreta_Este", new Vector3(20, 0, 0), TurretDirection.Este, baseModel, motorModel, radarModel, canonModel, projPrefab);
            TurretController_Arandia tOeste = CreateTurret("Torreta_Oeste", new Vector3(-20, 0, 0), TurretDirection.Oeste, baseModel, motorModel, radarModel, canonModel, projPrefab);

            // 7. Player
            GameObject player = CreatePlayer();
            RepairSystem_Arandia repairSystem = player.GetComponent<RepairSystem_Arandia>();

            // 8. Game Systems
            GameObject gameSystems = new GameObject("GameSystems");
            GameManager_Arandia gameManager = gameSystems.AddComponent<GameManager_Arandia>();
            WaveManager_Arandia waveManager = gameSystems.AddComponent<WaveManager_Arandia>();

            gameManager.turrets = new TurretController_Arandia[] { tNorte, tSur, tEste, tOeste };
            gameManager.waveManager = waveManager;

            // Spawner de piezas
            GameObject piecePrefab = CreatePiecePrefab();
            PieceSpawner_Arandia pieceSpawner = gameSystems.AddComponent<PieceSpawner_Arandia>();
            pieceSpawner.piecePrefab = piecePrefab;
            pieceSpawner.waveManager = waveManager;
            pieceSpawner.spawnRadius = 15f;
            pieceSpawner.piecesPerWave = 5;

            waveManager.turretDegradations = new TurretDegradation_Arandia[]
            {
                tNorte.GetComponent<TurretDegradation_Arandia>(),
                tSur.GetComponent<TurretDegradation_Arandia>(),
                tEste.GetComponent<TurretDegradation_Arandia>(),
                tOeste.GetComponent<TurretDegradation_Arandia>()
            };
            waveManager.gameManager = gameManager;

            // 9. HUD Manager
            GameObject hudManager = new GameObject("HUD_Manager");
            HUDBuilder_Arandia hudBuilder = hudManager.AddComponent<HUDBuilder_Arandia>();
            hudBuilder.turrets = gameManager.turrets;
            hudBuilder.repairSystem = repairSystem;
            hudBuilder.waveManager = waveManager;
            hudBuilder.gameManager = gameManager;

            Debug.Log("<color=green><b>[Last Machine] Reconstrucción de escena completada con éxito. ¡Listo para jugar!</b></color>");
        }

        private static void CreateTagsAndLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            AddTag(tagsProp, "Player");
            AddTag(tagsProp, "Enemy");

            SerializedProperty layersProp = tagManager.FindProperty("layers");
            AddLayer(layersProp, "Turret");

            tagManager.ApplyModifiedProperties();
        }

        private static void AddTag(SerializedProperty tagsProp, string tag)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag))
                {
                    found = true; break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                tagsProp.GetArrayElementAtIndex(0).stringValue = tag;
            }
        }

        private static void AddLayer(SerializedProperty layersProp, string layer)
        {
            bool found = false;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                if (layersProp.GetArrayElementAtIndex(i).stringValue.Equals(layer))
                {
                    found = true; break;
                }
            }
            if (!found)
            {
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    if (string.IsNullOrEmpty(layersProp.GetArrayElementAtIndex(i).stringValue))
                    {
                        layersProp.GetArrayElementAtIndex(i).stringValue = layer;
                        break;
                    }
                }
            }
        }

        private static GameObject LoadFBX(string folderName)
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { $"Assets/02_Model/TorretaArandia/{folderName}" });
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
            return null;
        }

        private static GameObject CreateProjectilePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03_Prefabs"))
                AssetDatabase.CreateFolder("Assets", "03_Prefabs");

            string[] guids = AssetDatabase.FindAssets("Projectile_Arandia t:Prefab", new[] { "Assets/03_Prefabs" });
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            GameObject proj = new GameObject("Projectile_Arandia");
            Projectile_Arandia script = proj.AddComponent<Projectile_Arandia>();
            script.speed = 40f; // Faster bullet
            script.damage = 25f;
            script.lifeTime = 4f;
            script.enemyTag = "Enemy";

            SphereCollider coll = proj.AddComponent<SphereCollider>();
            coll.isTrigger = true;
            coll.radius = 0.2f;

            Rigidbody rb = proj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // --- Visual de la bala (Capsula) ---
            GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            mesh.transform.SetParent(proj.transform, false);
            mesh.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
            mesh.transform.localRotation = Quaternion.Euler(90, 0, 0); // Apuntar hacia adelante
            Object.DestroyImmediate(mesh.GetComponent<Collider>());

            // Material Emisivo (Bala trazadora amarilla)
            Material bulletMat = new Material(Shader.Find("Standard"));
            bulletMat.color = Color.yellow;
            bulletMat.EnableKeyword("_EMISSION");
            bulletMat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0f) * 2f); // HDR intensity
            mesh.GetComponent<MeshRenderer>().sharedMaterial = bulletMat;

            // --- Estela de luz (Trail Renderer) ---
            TrailRenderer trail = proj.AddComponent<TrailRenderer>();
            trail.time = 0.1f; // Corto, tipo bala
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            
            // Material del trail (Unlit Color)
            Material trailMat = new Material(Shader.Find("Sprites/Default"));
            trailMat.color = new Color(1f, 0.7f, 0f, 0.8f);
            trail.sharedMaterial = trailMat;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(proj, "Assets/03_Prefabs/Projectile_Arandia.prefab");
            Object.DestroyImmediate(proj);
            return prefab;
        }

        private static GameObject CreatePiecePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/03_Prefabs"))
                AssetDatabase.CreateFolder("Assets", "03_Prefabs");

            string[] guids = AssetDatabase.FindAssets("Piece_Arandia t:Prefab", new[] { "Assets/03_Prefabs" });
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = "Piece_Arandia";
            piece.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            
            BoxCollider coll = piece.GetComponent<BoxCollider>();
            coll.isTrigger = true;

            PiecePickup_Arandia script = piece.AddComponent<PiecePickup_Arandia>();
            script.piecesAmount = 5;

            // Material metalico azulado
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.5f, 0.8f);
            mat.SetFloat("_Metallic", 1f);
            mat.SetFloat("_Glossiness", 0.8f);
            piece.GetComponent<MeshRenderer>().sharedMaterial = mat;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(piece, "Assets/03_Prefabs/Piece_Arandia.prefab");
            Object.DestroyImmediate(piece);
            return prefab;
        }

        private static TurretController_Arandia CreateTurret(string name, Vector3 position, TurretDirection dir, 
            GameObject baseM, GameObject motorM, GameObject radarM, GameObject canonM, GameObject projPrefab)
        {
            GameObject turretRoot = new GameObject(name);
            turretRoot.transform.position = position;
            turretRoot.layer = LayerMask.NameToLayer("Turret");

            // Rotar la raíz según la dirección
            switch (dir)
            {
                case TurretDirection.Norte: turretRoot.transform.rotation = Quaternion.Euler(0, 0, 0); break;
                case TurretDirection.Sur:   turretRoot.transform.rotation = Quaternion.Euler(0, 180, 0); break;
                case TurretDirection.Este:  turretRoot.transform.rotation = Quaternion.Euler(0, 90, 0); break;
                case TurretDirection.Oeste: turretRoot.transform.rotation = Quaternion.Euler(0, -90, 0); break;
            }

            // Base - Centrada
            GameObject baseInst = (GameObject)PrefabUtility.InstantiatePrefab(baseM);
            baseInst.name = "Base";
            baseInst.transform.SetParent(turretRoot.transform, false);
            baseInst.transform.localPosition = new Vector3(0f, 0.478f, 0f); // Centrado en Z
            baseInst.transform.localEulerAngles = new Vector3(-90f, 0f, 0f); // Ajuste típico de FBX

            // SensorPivot
            GameObject sensorPivot = new GameObject("SensorPivot");
            sensorPivot.transform.SetParent(turretRoot.transform, false);
            sensorPivot.transform.localPosition = new Vector3(0, 1.2f, 0); // Elevar un poco
            GameObject radarInst = (GameObject)PrefabUtility.InstantiatePrefab(radarM);
            radarInst.name = "Radar_Mesh";
            radarInst.transform.SetParent(sensorPivot.transform, false);
            radarInst.transform.localPosition = Vector3.zero;
            radarInst.transform.localEulerAngles = Vector3.zero;

            // CanonPivot
            GameObject canonPivot = new GameObject("CanonPivot");
            canonPivot.transform.SetParent(turretRoot.transform, false);
            canonPivot.transform.localPosition = new Vector3(0, 1.5f, 0.5f); // Posicion del cañón
            GameObject canonInst = (GameObject)PrefabUtility.InstantiatePrefab(canonM);
            canonInst.name = "Canon_Mesh";
            canonInst.transform.SetParent(canonPivot.transform, false);
            canonInst.transform.localPosition = Vector3.zero;
            canonInst.transform.localEulerAngles = Vector3.zero;

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(canonPivot.transform, false);
            firePoint.transform.localPosition = new Vector3(0f, 0f, 2f); // Punta del cañón

            // MotorPivot
            GameObject motorPivot = new GameObject("MotorPivot");
            motorPivot.transform.SetParent(turretRoot.transform, false);
            motorPivot.transform.localPosition = new Vector3(0, 0.5f, -0.5f); // Atrás de la base
            GameObject motorInst = (GameObject)PrefabUtility.InstantiatePrefab(motorM);
            motorInst.name = "Motor_Mesh";
            motorInst.transform.SetParent(motorPivot.transform, false);
            motorInst.transform.localPosition = Vector3.zero;
            motorInst.transform.localEulerAngles = Vector3.zero;

            // Componentes HP
            TurretComponent_Arandia sensorComp = new GameObject("Sensor_Component").AddComponent<TurretComponent_Arandia>();
            sensorComp.transform.SetParent(turretRoot.transform, false);
            sensorComp.componentType = ComponentType.Sensor;
            sensorComp.maxHP = 100f;

            TurretComponent_Arandia canonComp = new GameObject("Canon_Component").AddComponent<TurretComponent_Arandia>();
            canonComp.transform.SetParent(turretRoot.transform, false);
            canonComp.componentType = ComponentType.Canon;
            canonComp.maxHP = 100f;

            TurretComponent_Arandia motorComp = new GameObject("Motor_Component").AddComponent<TurretComponent_Arandia>();
            motorComp.transform.SetParent(turretRoot.transform, false);
            motorComp.componentType = ComponentType.Motor;
            motorComp.maxHP = 100f;

            // Controller
            TurretController_Arandia controller = turretRoot.AddComponent<TurretController_Arandia>();
            controller.direction = dir;
            controller.sensor = sensorComp;
            controller.canon = canonComp;
            controller.motor = motorComp;
            controller.firePoint = firePoint.transform;
            controller.projectilePrefab = projPrefab;
            controller.baseFireRate = 1f;
            controller.detectionRange = 10f;
            controller.baseDamage = 25f;

            // Degradation
            TurretDegradation_Arandia degradation = turretRoot.AddComponent<TurretDegradation_Arandia>();
            degradation.baseDamagePerSecond = 2f;
            degradation.waveScaling = 0.5f;

            // Animator
            TurretAnimator_Arandia animator = turretRoot.AddComponent<TurretAnimator_Arandia>();
            animator.canonPivot = canonPivot.transform;
            animator.sensorPivot = sensorPivot.transform;
            animator.motorPivot = motorPivot.transform;
            animator.firePoint = firePoint.transform;

            Renderer r1 = radarInst.GetComponentInChildren<Renderer>();
            Renderer r2 = canonInst.GetComponentInChildren<Renderer>();
            Renderer r3 = motorInst.GetComponentInChildren<Renderer>();
            animator.componentRenderers = new Renderer[] { r1, r2, r3 };
            
            // Collider
            SphereCollider sphere = turretRoot.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 3.5f;

            return controller;
        }

        private static GameObject CreatePlayer()
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0, 1, 0);
            player.tag = "Player";

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb == null) rb = player.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

            player.AddComponent<PlayerMovement_Arandia>();

            RepairSystem_Arandia repair = player.AddComponent<RepairSystem_Arandia>();
            PieceInventory_Arandia inv = player.AddComponent<PieceInventory_Arandia>();

            PlayerTurretConnector_Arandia conn = player.AddComponent<PlayerTurretConnector_Arandia>();
            conn.repairSystem = repair;
            conn.inventory = inv;
            conn.interactRadius = 3.5f;
            
            // Wait, LayerMask.NameToLayer("Turret") might be assigned in this same frame.
            // Using 1 << 6 is risky if Turret is not layer 6.
            // We'll use NameToLayer since we just applied properties.
            conn.turretLayer = 1 << LayerMask.NameToLayer("Turret");

            return player;
        }
    }
}
