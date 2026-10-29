using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Menú "Diana > Montar escena": crea Assets/Scenes/DianaDelBosque.unity con todo el juego
// montado siguiendo la guía (mesas, pistola, pelotas, latas, dianas, pedestal, marcador...).
// No modifica las escenas que ya tengas.
public static class MontadorDiana
{
    const string Raiz = "Assets/Diana";
    const string CarpetaMateriales = Raiz + "/Materiales";
    const string CarpetaPrefabs = Raiz + "/Prefabs";
    const string RutaEscena = "Assets/Scenes/DianaDelBosque.unity";
    const string RutaPistola = "Assets/PolyGun/Modern Pistol Pack/Prefabs/Pistol_05.prefab";
    const string RutaRPG = "Assets/RPGPP_LT/Prefabs/";
    const string RutaSonidos = "Assets/Casual Game Sounds U6/CasualGameSounds/";
    const string GuidSkyboxElegido = "24d76106ebc1bd947a3fed862f899b82";
    const int CapaTeleport = 31;

    [MenuItem("Diana/Montar escena")]
    public static void Montar()
    {
        if (System.IO.File.Exists(RutaEscena) &&
            !EditorUtility.DisplayDialog("Diana del Bosque",
                "Ya existe " + RutaEscena + ". ¿Quieres sustituirla por una nueva?", "Sustituir", "Cancelar"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string rutaXROrigin = BuscarAsset("XR Origin (XR Rig) t:Prefab", "Starter Assets");
        if (rutaXROrigin == null)
        {
            EditorUtility.DisplayDialog("Diana del Bosque",
                "No encuentro el prefab XR Origin (XR Rig). Importa el sample Starter Assets de XR Interaction Toolkit.", "Vale");
            return;
        }

        AsegurarTag("Proyectil");
        int capaGema = AsegurarCapaInteraccion("Gema");
        CrearCarpeta(Raiz, "Materiales");
        CrearCarpeta(Raiz, "Prefabs");

        var matHierba = CrearMaterial("Hierba", new Color(0.36f, 0.55f, 0.27f));
        var matMadera = CrearMaterial("Madera", new Color(0.55f, 0.37f, 0.2f));
        var matAncla = CrearMaterial("Ancla", new Color(0.2f, 0.75f, 0.5f));
        var matPiedra = CrearMaterial("Piedra", new Color(0.55f, 0.55f, 0.58f));
        var matMetal = CrearMaterial("MetalPistola", new Color(0.18f, 0.18f, 0.2f), 0.8f, 0.6f);
        var matBala = CrearMaterial("Bala", new Color(1f, 0.8f, 0.2f), 0.5f, 0.6f);
        var matPelota = CrearMaterial("Pelota", new Color(0.95f, 0.95f, 0.95f));
        var matLata = CrearMaterial("Lata", new Color(0.85f, 0.15f, 0.15f), 0.7f, 0.5f);
        var matDianaRoja = CrearMaterial("DianaRoja", new Color(0.85f, 0.1f, 0.1f));
        var matDianaBlanca = CrearMaterial("DianaBlanca", Color.white);
        var matGema = CrearMaterial("Gema", new Color(0.55f, 0.2f, 0.95f), 0.2f, 0.9f);
        matGema.EnableKeyword("_EMISSION");
        matGema.SetColor("_EmissionColor", new Color(0.35f, 0.1f, 0.6f));

        var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var camara = GameObject.Find("Main Camera");
        if (camara) Object.DestroyImmediate(camara);
        var luz = Object.FindFirstObjectByType<Light>();
        if (luz) luz.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        var skybox = CargarSkybox();
        if (skybox) RenderSettings.skybox = skybox;

        // --- Suelo y rig -------------------------------------------------------------
        var suelo = GameObject.CreatePrimitive(PrimitiveType.Plane);
        suelo.name = "Suelo";
        suelo.transform.localScale = new Vector3(4f, 1f, 4f); // 40 x 40 m
        suelo.GetComponent<Renderer>().sharedMaterial = matHierba;
        suelo.isStatic = true;
        var area = suelo.AddComponent<TeleportationArea>();
        area.interactionLayers = 1 << CapaTeleport;

        var rig = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(rutaXROrigin));
        rig.transform.position = new Vector3(0f, 0f, -2f);

        CrearAncla("Ancla Inicio", new Vector3(0f, 0f, -2f), matAncla);
        CrearAncla("Ancla A (Tiro)", new Vector3(-4f, 0f, 2.2f), matAncla);
        CrearAncla("Ancla B (Latas)", new Vector3(4f, 0f, 2.2f), matAncla);

        // --- Mesas ------------------------------------------------------------------
        CrearCubo("Mesa Tiro", new Vector3(-4f, 0.45f, 3f), new Vector3(1.2f, 0.9f, 0.6f), matMadera, true);
        CrearCubo("Mesa Latas", new Vector3(4f, 0.45f, 3f), new Vector3(1.2f, 0.9f, 0.6f), matMadera, true);
        CrearCubo("Liston delantero", new Vector3(4f, 0.93f, 2.72f), new Vector3(1.2f, 0.06f, 0.03f), matMadera, true);
        CrearCubo("Liston trasero", new Vector3(4f, 0.93f, 3.28f), new Vector3(1.2f, 0.06f, 0.03f), matMadera, true);

        // --- Pistola ----------------------------------------------------------------
        var balaPrefab = CrearPrefabBala(matBala);
        var clipDisparo = Sonido("DM-CGS-01");
        CrearPistola(balaPrefab, matMetal, clipDisparo);

        // --- Pelotas y latas ----------------------------------------------------------
        var pelotas = new GameObject("Pelotas");
        for (int i = 0; i < 5; i++)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            p.name = "Pelota " + (i + 1);
            p.tag = "Proyectil";
            p.transform.SetParent(pelotas.transform);
            p.transform.position = new Vector3(3.6f + i * 0.2f, 0.98f, 3f);
            p.transform.localScale = Vector3.one * 0.1f;
            p.GetComponent<Renderer>().sharedMaterial = matPelota;
            var grab = p.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = true;
            grab.useDynamicAttach = true;
            var rb = p.GetComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        CrearCubo("Peana latas", new Vector3(4f, 0.5f, 6f), new Vector3(1f, 1f, 0.5f), matMadera, true);
        var latas = new GameObject("Latas");
        var clipLata = Sonido("DM-CGS-21");
        int[] filas = { 3, 2, 1 };
        for (int fila = 0; fila < filas.Length; fila++)
        {
            for (int j = 0; j < filas[fila]; j++)
            {
                float x = 4f + (j - (filas[fila] - 1) * 0.5f) * 0.1f;
                float y = 1.0f + 0.061f + fila * 0.122f;
                var lata = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lata.name = "Lata";
                lata.transform.SetParent(latas.transform);
                lata.transform.position = new Vector3(x, y, 6f);
                lata.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f); // 8 cm x 12 cm
                lata.GetComponent<Renderer>().sharedMaterial = matLata;
                CambiarAColliderConvexo(lata);
                var rb = lata.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                var comp = lata.AddComponent<Lata>();
                Asignar(comp, "sonidoCaida", clipLata);
            }
        }

        // --- Dianas -------------------------------------------------------------------
        var dianas = new GameObject("Dianas");
        var clipDiana = Sonido("DM-CGS-20");
        CrearDiana(dianas.transform, new Vector3(-6f, 1.3f, 11f), new Vector3(2f, 0f, 0f), 0.4f, matDianaRoja, matDianaBlanca, clipDiana);
        CrearDiana(dianas.transform, new Vector3(-4f, 1.8f, 12.5f), new Vector3(3f, 0f, 0f), 0.6f, matDianaRoja, matDianaBlanca, clipDiana);
        CrearDiana(dianas.transform, new Vector3(-2f, 1.2f, 11.5f), new Vector3(0f, 0.8f, 0f), 0.5f, matDianaRoja, matDianaBlanca, clipDiana);

        // --- Gema y pedestal con socket -----------------------------------------------
        var soporte = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        soporte.name = "Soporte gema";
        soporte.transform.position = new Vector3(-0.8f, 0.45f, -1.4f);
        soporte.transform.localScale = new Vector3(0.25f, 0.45f, 0.25f);
        soporte.GetComponent<Renderer>().sharedMaterial = matPiedra;

        var gema = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gema.name = "Gema";
        gema.transform.position = new Vector3(-0.8f, 0.97f, -1.4f);
        gema.transform.rotation = Quaternion.Euler(45f, 0f, 45f);
        gema.transform.localScale = Vector3.one * 0.08f;
        gema.GetComponent<Renderer>().sharedMaterial = matGema;
        var grabGema = gema.AddComponent<XRGrabInteractable>();
        grabGema.interactionLayers = 1 | (1 << capaGema); // Default + Gema
        grabGema.useDynamicAttach = true;
        gema.GetComponent<Rigidbody>().mass = 0.2f;

        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Pedestal";
        pedestal.transform.position = new Vector3(0.8f, 0.5f, -1.4f);
        pedestal.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        pedestal.GetComponent<Renderer>().sharedMaterial = matPiedra;

        var socketGO = new GameObject("Socket");
        socketGO.transform.position = new Vector3(0.8f, 1.1f, -1.4f);
        var trigger = socketGO.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.15f;
        var socket = socketGO.AddComponent<XRSocketInteractor>();
        socket.interactionLayers = 1 << capaGema; // solo la gema arranca la partida
        socket.showInteractableHoverMeshes = true;

        // --- Marcador (UI en el espacio) ---------------------------------------------
        TMP_Text textoPuntos, textoTiempo, textoMensaje;
        Button boton;
        CrearMarcador(out textoPuntos, out textoTiempo, out textoMensaje, out boton);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<XRUIInputModule>();

        // --- GameManager ---------------------------------------------------------------
        var gmGO = new GameObject("GameManager");
        var gm = gmGO.AddComponent<GameManager>();
        var audioInicio = gmGO.AddComponent<AudioSource>();
        audioInicio.playOnAwake = false;
        audioInicio.clip = Sonido("DM-CGS-26");
        var audioFin = gmGO.AddComponent<AudioSource>();
        audioFin.playOnAwake = false;
        audioFin.clip = Sonido("DM-CGS-18");
        Asignar(gm, "textoPuntos", textoPuntos);
        Asignar(gm, "textoTiempo", textoTiempo);
        Asignar(gm, "textoMensaje", textoMensaje);
        Asignar(gm, "botonReiniciar", boton.gameObject);
        Asignar(gm, "audioInicio", audioInicio);
        Asignar(gm, "audioFin", audioFin);

        UnityEventTools.AddVoidPersistentListener(socket.selectEntered, gm.EmpezarPartida);
        UnityEventTools.AddPersistentListener(boton.onClick, gm.Reiniciar);

        // --- Decorado --------------------------------------------------------------------
        CrearDecorado();

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AnadirABuild(RutaEscena);
        AssetDatabase.SaveAssets();

        Debug.Log("[Diana] Escena montada en " + RutaEscena + ". Pulsa Play para probarla con el simulador.");
        EditorUtility.DisplayDialog("Diana del Bosque",
            "Escena montada y guardada en " + RutaEscena + ".\n\nPulsa Play: coge la gema (izquierda) y déjala en el pedestal (derecha) para empezar.", "Vale");
    }

    // ===================================================================================

    static void CrearPistola(Rigidbody balaPrefab, Material matMetal, AudioClip clipDisparo)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPistola);
        GameObject pistola;
        if (prefab != null)
        {
            pistola = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            // El shader "PolyGun/Gun Shader" es del pipeline Built-in y en URP se ve rosa.
            // Ni el Render Pipeline Converter lo convierte, así que le ponemos un material URP.
            foreach (var r in pistola.GetComponentsInChildren<Renderer>())
                r.sharedMaterials = Enumerable.Repeat(matMetal, r.sharedMaterials.Length).ToArray();
        }
        else
        {
            Debug.LogWarning("[Diana] No encuentro " + RutaPistola + ". Uso una pistola hecha con cubos.");
            pistola = new GameObject("Pistola");
            var cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cuerpo.transform.SetParent(pistola.transform);
            cuerpo.transform.localPosition = new Vector3(0f, 0.05f, 0.04f);
            cuerpo.transform.localScale = new Vector3(0.03f, 0.04f, 0.18f);
            var mango = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mango.transform.SetParent(pistola.transform);
            mango.transform.localPosition = new Vector3(0f, -0.01f, -0.02f);
            mango.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
            mango.transform.localScale = new Vector3(0.028f, 0.1f, 0.04f);
            foreach (var c in pistola.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (var r in pistola.GetComponentsInChildren<Renderer>()) r.sharedMaterial = matMetal;
        }
        pistola.name = "Pistola";
        pistola.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Si el modelo no mide lo que una pistola real (~20 cm), lo escalamos
        Bounds b = Limites(pistola);
        float mayor = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (mayor > 0f && (mayor < 0.1f || mayor > 0.5f))
        {
            pistola.transform.localScale *= 0.22f / mayor;
            b = Limites(pistola);
        }
        float s = pistola.transform.localScale.x;

        var col = pistola.AddComponent<BoxCollider>();
        col.center = b.center / s;
        col.size = b.size / s;

        // Punto de agarre en la empuñadura (parte baja y trasera) y boca delante del cañón (+Z)
        var agarre = new GameObject("Agarre").transform;
        agarre.SetParent(pistola.transform, false);
        agarre.localPosition = new Vector3(b.center.x, b.center.y - b.extents.y * 0.4f, b.center.z - b.extents.z * 0.5f) / s;

        var boca = new GameObject("Boca").transform;
        boca.SetParent(pistola.transform, false);
        boca.localPosition = new Vector3(b.center.x, b.max.y - b.size.y * 0.2f, b.max.z + 0.04f) / s;

        var grab = pistola.AddComponent<XRGrabInteractable>();
        grab.attachTransform = agarre;
        grab.useDynamicAttach = false;
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.throwOnDetach = true;
        var rb = pistola.GetComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var audio = pistola.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 1f;
        audio.clip = clipDisparo;

        var comp = pistola.AddComponent<Pistola>();
        Asignar(comp, "proyectilPrefab", balaPrefab);
        Asignar(comp, "boca", boca);
        Asignar(comp, "audioDisparo", audio);

        // Tumbada sobre la Mesa Tiro, con el cañón hacia las dianas
        pistola.transform.SetPositionAndRotation(new Vector3(-4f, 0.9f + b.extents.x + 0.01f, 3f), Quaternion.Euler(0f, 0f, 90f));
    }

    static Rigidbody CrearPrefabBala(Material mat)
    {
        var bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "Bala";
        bala.tag = "Proyectil";
        bala.transform.localScale = Vector3.one * 0.03f;
        bala.GetComponent<Renderer>().sharedMaterial = mat;
        var rb = bala.AddComponent<Rigidbody>();
        rb.mass = 0.05f;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        var prefab = PrefabUtility.SaveAsPrefabAsset(bala, CarpetaPrefabs + "/Bala.prefab");
        Object.DestroyImmediate(bala);
        return prefab.GetComponent<Rigidbody>();
    }

    static void CrearDiana(Transform padre, Vector3 pos, Vector3 recorrido, float velocidad,
        Material roja, Material blanca, AudioClip clip)
    {
        var diana = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        diana.name = "Diana";
        diana.transform.SetParent(padre);
        diana.transform.position = pos;
        diana.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // la cara mira al jugador
        diana.transform.localScale = new Vector3(0.5f, 0.02f, 0.5f);
        diana.GetComponent<Renderer>().sharedMaterial = roja;
        CambiarAColliderConvexo(diana);
        var rb = diana.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        var centro = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        centro.name = "Centro";
        Object.DestroyImmediate(centro.GetComponent<Collider>());
        centro.transform.SetParent(diana.transform, false);
        centro.transform.localPosition = new Vector3(0f, -1.1f, 0f); // hacia el jugador
        centro.transform.localScale = new Vector3(0.45f, 0.2f, 0.45f);
        centro.GetComponent<Renderer>().sharedMaterial = blanca;

        var comp = diana.AddComponent<Diana>();
        Asignar(comp, "sonidoImpacto", clip);
        var mover = diana.AddComponent<MoverDiana>();
        var so = new SerializedObject(mover);
        so.FindProperty("recorrido").vector3Value = recorrido;
        so.FindProperty("velocidad").floatValue = velocidad;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void CrearAncla(string nombre, Vector3 pos, Material mat)
    {
        var ancla = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ancla.name = nombre;
        ancla.transform.position = pos + Vector3.up * 0.02f;
        ancla.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
        ancla.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(ancla.GetComponent<Collider>());
        var box = ancla.AddComponent<BoxCollider>(); // la cápsula del cilindro se deforma al aplanarlo
        box.size = new Vector3(1f, 2f, 1f);
        var anchor = ancla.AddComponent<TeleportationAnchor>();
        anchor.interactionLayers = 1 << CapaTeleport;
        anchor.matchOrientation = MatchOrientation.TargetUpAndForward; // llegas mirando hacia +Z
    }

    static void CrearMarcador(out TMP_Text puntos, out TMP_Text tiempo, out TMP_Text mensaje, out Button boton)
    {
        var canvasGO = new GameObject("Marcador", typeof(RectTransform));
        canvasGO.transform.position = new Vector3(0f, 1.8f, 4.5f); // entre los dos puestos, visible desde todos
        canvasGO.transform.localScale = Vector3.one * 0.0025f;
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 10f;
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();
        var rt = (RectTransform)canvasGO.transform;
        rt.sizeDelta = new Vector2(900f, 520f);

        var fondo = canvasGO.AddComponent<Image>();
        fondo.color = new Color(0.08f, 0.12f, 0.1f, 0.85f);

        puntos = Texto(canvasGO.transform, "Puntos", "Puntos: 0", 72, new Vector2(-200f, 170f), new Vector2(420f, 100f));
        tiempo = Texto(canvasGO.transform, "Tiempo", "Tiempo: 90", 72, new Vector2(200f, 170f), new Vector2(420f, 100f));
        mensaje = Texto(canvasGO.transform, "Mensaje", "Coloca la gema en el pedestal para empezar", 44, new Vector2(0f, 20f), new Vector2(840f, 180f));

        var botonGO = new GameObject("Boton Otra vez", typeof(RectTransform));
        botonGO.transform.SetParent(canvasGO.transform, false);
        var brt = (RectTransform)botonGO.transform;
        brt.anchoredPosition = new Vector2(0f, -170f);
        brt.sizeDelta = new Vector2(360f, 110f);
        var img = botonGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.65f, 0.42f);
        boton = botonGO.AddComponent<Button>();
        boton.targetGraphic = img;
        var t = Texto(botonGO.transform, "Texto", "Otra vez", 52, Vector2.zero, brt.sizeDelta);
        t.color = Color.white;
    }

    static TMP_Text Texto(Transform padre, string nombre, string contenido, float tam, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        var rt = (RectTransform)go.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = contenido;
        t.fontSize = tam;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(0.9f, 0.95f, 0.92f);
        return t;
    }

    static void CrearDecorado()
    {
        var decorado = new GameObject("Decorado");
        string[] arboles =
        {
            "Nature/Vegetation/Trees/rpgpp_lt_tree_01", "Nature/Vegetation/Trees/rpgpp_lt_tree_02",
            "Nature/Vegetation/Trees/rpgpp_lt_tree_pine_01"
        };
        string[] extras =
        {
            "Nature/Rocks/rpgpp_lt_rock_01", "Nature/Rocks/rpgpp_lt_rock_02", "Nature/Rocks/rpgpp_lt_rock_03",
            "Nature/Vegetation/Bushes/rpgpp_lt_bush_01", "Nature/Vegetation/Bushes/rpgpp_lt_bush_02"
        };
        var azar = new System.Random(7);
        int total = 28;
        for (int i = 0; i < total; i++)
        {
            float angulo = i * Mathf.PI * 2f / total + (float)azar.NextDouble() * 0.15f;
            var dir = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo));
            float radio = 12f + (float)azar.NextDouble() * 2f;
            // En el lado de las dianas (norte) los árboles van más lejos para hacer de fondo
            if (dir.z > 0.5f) radio = Mathf.Min(Mathf.Max(radio, 16.5f / dir.z), 19f);
            string ruta = (i % 3 == 2) ? extras[azar.Next(extras.Length)] : arboles[azar.Next(arboles.Length)];
            var go = InstanciarRPG(ruta, decorado.transform);
            if (go == null) continue;
            go.transform.position = dir * radio;
            go.transform.rotation = Quaternion.Euler(0f, azar.Next(360), 0f);
            go.transform.localScale *= 0.8f + (float)azar.NextDouble() * 0.5f;
        }
        foreach (var t in decorado.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;
    }

    static GameObject InstanciarRPG(string ruta, Transform padre)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaRPG + ruta + ".prefab");
        if (prefab == null)
        {
            Debug.LogWarning("[Diana] No encuentro " + RutaRPG + ruta + ".prefab");
            return null;
        }
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
        return go;
    }

    // ===================================================================================

    static GameObject CrearCubo(string nombre, Vector3 pos, Vector3 escala, Material mat, bool estatico)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.position = pos;
        go.transform.localScale = escala;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        go.isStatic = estatico;
        return go;
    }

    static void CambiarAColliderConvexo(GameObject go)
    {
        Object.DestroyImmediate(go.GetComponent<Collider>());
        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        mc.convex = true;
    }

    static Bounds Limites(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one * 0.1f);
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        b.center -= go.transform.position;
        return b;
    }

    static void Asignar(Object componente, string campo, Object valor)
    {
        var so = new SerializedObject(componente);
        var p = so.FindProperty(campo);
        if (p == null)
        {
            Debug.LogWarning("[Diana] El campo " + campo + " no existe en " + componente.GetType().Name);
            return;
        }
        p.objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static Material CrearMaterial(string nombre, Color color, float metalico = 0f, float suavidad = 0.3f)
    {
        string ruta = CarpetaMateriales + "/" + nombre + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, ruta);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metalico);
        mat.SetFloat("_Smoothness", suavidad);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static AudioClip Sonido(string nombre)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(RutaSonidos + nombre + ".wav");
        if (clip == null) Debug.LogWarning("[Diana] No encuentro el sonido " + nombre);
        return clip;
    }

    static Material CargarSkybox()
    {
        string ruta = AssetDatabase.GUIDToAssetPath(GuidSkyboxElegido);
        var mat = string.IsNullOrEmpty(ruta) ? null : AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat != null) return mat;
        foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Fantasy Skybox FREE" }))
        {
            mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (mat != null && mat.shader != null && mat.shader.name.StartsWith("Skybox")) return mat;
        }
        return null;
    }

    static string BuscarAsset(string filtro, string debeContener)
    {
        foreach (var guid in AssetDatabase.FindAssets(filtro))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (ruta.Contains(debeContener)) return ruta;
        }
        return null;
    }

    static void CrearCarpeta(string padre, string nombre)
    {
        if (!AssetDatabase.IsValidFolder(padre))
            AssetDatabase.CreateFolder("Assets", padre.Substring("Assets/".Length));
        if (!AssetDatabase.IsValidFolder(padre + "/" + nombre))
            AssetDatabase.CreateFolder(padre, nombre);
    }

    static void AsegurarTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    // Busca (o crea en el primer hueco libre) una capa de interacción de XRI con ese nombre
    static int AsegurarCapaInteraccion(string nombre)
    {
        string ruta = BuscarAsset("InteractionLayerSettings", "InteractionLayerSettings.asset")
                      ?? "Assets/XRI/Settings/Resources/InteractionLayerSettings.asset";
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ruta);
        if (asset == null)
        {
            Debug.LogWarning("[Diana] No encuentro InteractionLayerSettings; uso la capa 1 para la gema. Nómbrala 'Gema' a mano.");
            return 1;
        }
        var so = new SerializedObject(asset);
        var nombres = so.FindProperty("m_LayerNames");
        for (int i = 1; i < nombres.arraySize; i++)
            if (nombres.GetArrayElementAtIndex(i).stringValue == nombre) return i;
        for (int i = 1; i < nombres.arraySize - 1; i++)
        {
            if (!string.IsNullOrEmpty(nombres.GetArrayElementAtIndex(i).stringValue)) continue;
            nombres.GetArrayElementAtIndex(i).stringValue = nombre;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return i;
        }
        Debug.LogWarning("[Diana] No hay capas de interacción libres; uso la capa 1.");
        return 1;
    }

    static void AnadirABuild(string ruta)
    {
        var escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        escenas.RemoveAll(e => e.path == ruta);
        escenas.Insert(0, new EditorBuildSettingsScene(ruta, true));
        EditorBuildSettings.scenes = escenas.ToArray();
    }
}
