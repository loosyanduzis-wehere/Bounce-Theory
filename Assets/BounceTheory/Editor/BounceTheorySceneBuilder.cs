using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BounceTheory.Editor
{
    public static class BounceTheorySceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/BounceTheoryPrototype.unity";
        private const string MaterialFolder = "Assets/BounceTheory/Materials";
        private const string PrototypeBounceAudioPath = "Assets/BounceTheory/Audio/PrototypeBounce.wav";
        private const string ReferencePulseAudioPath = "Assets/BounceTheory/Audio/ReferencePulse.wav";

        [MenuItem("Bounce Theory/Build Chunk 1 Prototype Scene")]
        public static void BuildScene()
        {
            EnsureFolder("Assets", "BounceTheory");
            EnsureFolder("Assets/BounceTheory", "Editor");
            EnsureFolder("Assets/BounceTheory", "Materials");

            var m = CreateMaterials();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.25f, 0.34f, 0.46f);
            RenderSettings.ambientEquatorColor = new Color(0.13f, 0.16f, 0.22f);
            RenderSettings.ambientGroundColor = new Color(0.055f, 0.065f, 0.085f);
            RenderSettings.ambientIntensity = 0.9f;
            RenderSettings.fog = false;

            Transform root = Group("BounceTheoryPrototype", null);
            Court(root, m);
            Player(root, m);
            Ball(root, m);
            Defender(root, m);
            Basket(root, m);
            Lights(root);
            Camera(root);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Selection.activeGameObject = root.gameObject;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Bounce Theory Chunk 1 scene created: " + ScenePath);
        }

        public static void BuildAndCapture()
        {
            BuildScene();
            CapturePreview();
        }

        [MenuItem("Bounce Theory/Upgrade Prototype Scene To Chunk 2")]
        public static void UpgradeToChunk2()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform offense = GameObject.Find("OffensivePlayer")?.transform;
            Transform ball = GameObject.Find("Basketball")?.transform;
            Transform defender = GameObject.Find("Defender")?.transform;
            Transform cameraTransform = GameObject.Find("Main Camera")?.transform;

            if (!prototype || !offense || !ball || !defender || !cameraTransform)
                throw new InvalidOperationException("Chunk 1 scene is incomplete; cannot apply the Chunk 2 upgrade.");

            Vector3 offensePosition = offense.position;
            Vector3 defenderPosition = defender.position;
            Vector3 cameraPosition = cameraTransform.position;
            Quaternion cameraRotation = cameraTransform.rotation;

            EnsureFolder("Assets", "BounceTheory");
            EnsureFolder("Assets/BounceTheory", "Materials");
            AddChunk2(prototype, offense, ball, CreateMaterials());

            AssertUnchanged("OffensivePlayer position", offensePosition, offense.position);
            AssertUnchanged("Defender position", defenderPosition, defender.position);
            AssertUnchanged("Main Camera position", cameraPosition, cameraTransform.position);
            if (Quaternion.Angle(cameraRotation, cameraTransform.rotation) > 0.001f)
                throw new InvalidOperationException("Chunk 2 upgrade changed the Main Camera rotation.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);

            AssetDatabase.SaveAssets();
            Debug.Log("Bounce Theory Chunk 2 upgrade applied without changing the Chunk 1 player, defender, or camera transforms.");
        }

        [MenuItem("Bounce Theory/Upgrade Prototype Scene To Chunk 3")]
        public static void UpgradeToChunk3()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform offense = GameObject.Find("OffensivePlayer")?.transform;
            Transform defender = GameObject.Find("Defender")?.transform;
            Transform ball = GameObject.Find("Basketball")?.transform;
            Transform cameraTransform = GameObject.Find("Main Camera")?.transform;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            if (!prototype || !offense || !defender || !ball || !cameraTransform || !controller)
                throw new InvalidOperationException("Chunk 1/2 scene is incomplete; cannot apply the Chunk 3 upgrade.");

            Vector3 offensePosition = offense.position;
            Vector3 defenderPosition = defender.position;
            Vector3 cameraPosition = cameraTransform.position;
            Quaternion cameraRotation = cameraTransform.rotation;

            Transform rhythmObject = prototype.Find("RhythmClock");
            if (!rhythmObject) rhythmObject = Group("RhythmClock", prototype);
            RhythmClock clock = rhythmObject.GetComponent<RhythmClock>();
            if (!clock) clock = rhythmObject.gameObject.AddComponent<RhythmClock>();

            AudioSource audioSource = ball.GetComponent<AudioSource>();
            if (!audioSource) audioSource = ball.gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0.25f;
            audioSource.dopplerLevel = 0f;

            AudioClip prototypeBounce = CreatePrototypeBounceAudio();
            controller.ConfigureRhythm(clock, audioSource, prototypeBounce);
            EditorUtility.SetDirty(clock);
            EditorUtility.SetDirty(audioSource);
            EditorUtility.SetDirty(controller);

            AssertUnchanged("OffensivePlayer position", offensePosition, offense.position);
            AssertUnchanged("Defender position", defenderPosition, defender.position);
            AssertUnchanged("Main Camera position", cameraPosition, cameraTransform.position);
            if (Quaternion.Angle(cameraRotation, cameraTransform.rotation) > 0.001f)
                throw new InvalidOperationException("Chunk 3 upgrade changed the Main Camera rotation.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Bounce Theory Chunk 3 rhythm upgrade applied without changing player, defender, or camera transforms.");
        }

        [MenuItem("Bounce Theory/Upgrade Prototype Scene To Chunk 4")]
        public static void UpgradeToChunk4()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform offense = GameObject.Find("OffensivePlayer")?.transform;
            Transform defender = GameObject.Find("Defender")?.transform;
            Transform ball = GameObject.Find("Basketball")?.transform;
            Transform cameraTransform = GameObject.Find("Main Camera")?.transform;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            Transform rhythmObject = prototype ? prototype.Find("RhythmClock") : null;
            RhythmClock clock = rhythmObject ? rhythmObject.GetComponent<RhythmClock>() : null;
            if (!prototype || !offense || !defender || !ball || !cameraTransform || !controller || !clock)
                throw new InvalidOperationException("Chunks 1-3 are incomplete; cannot apply the Chunk 4 upgrade.");

            Vector3 offensePosition = offense.position;
            Vector3 defenderPosition = defender.position;
            Vector3 cameraPosition = cameraTransform.position;
            Quaternion cameraRotation = cameraTransform.rotation;

            clock.SetBpm(130f);
            AudioClip referencePulse = CreateReferencePulseAudio();
            AudioSource[] voices = rhythmObject.GetComponents<AudioSource>();
            AudioSource first = voices.Length > 0 ? voices[0] : rhythmObject.gameObject.AddComponent<AudioSource>();
            AudioSource second = voices.Length > 1 ? voices[1] : rhythmObject.gameObject.AddComponent<AudioSource>();
            ReferenceGroovePlayer groove = rhythmObject.GetComponent<ReferenceGroovePlayer>();
            if (!groove) groove = rhythmObject.gameObject.AddComponent<ReferenceGroovePlayer>();
            groove.Configure(clock, first, second, referencePulse);
            groove.SetVolume(0.16f);
            groove.SetMuted(false);

            EditorUtility.SetDirty(clock);
            EditorUtility.SetDirty(first);
            EditorUtility.SetDirty(second);
            EditorUtility.SetDirty(groove);

            AssertUnchanged("OffensivePlayer position", offensePosition, offense.position);
            AssertUnchanged("Defender position", defenderPosition, defender.position);
            AssertUnchanged("Main Camera position", cameraPosition, cameraTransform.position);
            if (Quaternion.Angle(cameraRotation, cameraTransform.rotation) > 0.001f)
                throw new InvalidOperationException("Chunk 4 upgrade changed the Main Camera rotation.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Bounce Theory Chunk 4 rhythmic grid and synchronized reference groove applied without changing player, defender, or camera transforms.");
        }

        [MenuItem("Bounce Theory/Upgrade Prototype Scene To Chunk 4.5")]
        public static void UpgradeToChunk45()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject offense = GameObject.Find("OffensivePlayer");
            GameObject defender = GameObject.Find("Defender");
            GameObject ball = GameObject.Find("Basketball");
            GameObject cameraObject = GameObject.Find("Main Camera");
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            if (!offense || !defender || !ball || !cameraObject || !controller)
                throw new InvalidOperationException("Chunks 1-4 are incomplete; cannot apply the Chunk 4.5 upgrade.");

            Vector3 offensePosition = offense.transform.position;
            Vector3 defenderPosition = defender.transform.position;
            Vector3 cameraPosition = cameraObject.transform.position;
            Quaternion cameraRotation = cameraObject.transform.rotation;

            controller.ConfigureReadability(1.56f);
            EditorUtility.SetDirty(controller);

            AssertUnchanged("OffensivePlayer position", offensePosition, offense.transform.position);
            AssertUnchanged("Defender position", defenderPosition, defender.transform.position);
            AssertUnchanged("Main Camera position", cameraPosition, cameraObject.transform.position);
            if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > .001f)
                throw new InvalidOperationException("Chunk 4.5 upgrade changed the Main Camera rotation.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Bounce Theory Chunk 4.5 input-authority and ball-readability upgrade applied.");
        }

        [MenuItem("Bounce Theory/Upgrade Prototype Scene To Chunk 4.75")]
        public static void UpgradeToChunk475()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject offense = GameObject.Find("OffensivePlayer");
            GameObject defender = GameObject.Find("Defender");
            GameObject ball = GameObject.Find("Basketball");
            GameObject cameraObject = GameObject.Find("Main Camera");
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            if (!offense || !defender || !ball || !cameraObject || !controller)
                throw new InvalidOperationException("Chunks 1-4.5 are incomplete; cannot apply the Chunk 4.75 upgrade.");

            Vector3 offensePosition = offense.transform.position;
            Vector3 defenderPosition = defender.transform.position;
            Vector3 cameraPosition = cameraObject.transform.position;
            Quaternion cameraRotation = cameraObject.transform.rotation;

            controller.ConfigureReadability(1.56f);
            controller.ConfigureAdaptiveMotion(1.0f, 8.0f, .65f, .08f);
            EditorUtility.SetDirty(controller);

            AssertUnchanged("OffensivePlayer position", offensePosition, offense.transform.position);
            AssertUnchanged("Defender position", defenderPosition, defender.transform.position);
            AssertUnchanged("Main Camera position", cameraPosition, cameraObject.transform.position);
            if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > .001f)
                throw new InvalidOperationException("Chunk 4.75 upgrade changed the Main Camera rotation.");

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Bounce Theory Chunk 4.75 target-contact timing and adaptive motion upgrade applied.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 1 Prototype Scene")]
        public static void ValidateScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject defender = Required("Defender", errors);
            GameObject basket = Required("Basket", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Required("BounceTheoryPrototype", errors);

            if (offense && defender && basket &&
                !(offense.transform.position.z < defender.transform.position.z && defender.transform.position.z < basket.transform.position.z))
                errors.Add("Defender is not between offense and basket.");

            if (cameraObject && offense)
            {
                if (cameraObject.transform.position.z >= offense.transform.position.z)
                    errors.Add("Main Camera is not behind the offensive player.");
                UnityEngine.Camera cam = cameraObject.GetComponent<UnityEngine.Camera>();
                if (!cam) errors.Add("Main Camera has no Camera component.");
                else
                {
                    Visible(cam, offense, "OffensivePlayer", errors);
                    Visible(cam, ball, "Basketball", errors);
                    Visible(cam, defender, "Defender", errors);
                    Visible(cam, basket, "Basket", errors);
                }
            }

            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>())
                errors.Add("A CharacterController exists in the scene.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>())
                errors.Add("A Rigidbody exists in the scene; Chunk 1 must remain stationary.");
            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 1 validation failed:\n- " + string.Join("\n- ", errors));

            Debug.Log("Bounce Theory Chunk 1 validation passed: required objects exist; camera framing and subject order are correct; no CharacterController or Rigidbody is present.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 2 Pound Dribble")]
        public static void ValidateChunk2()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;

            if (!controller) errors.Add("Basketball has no PoundDribbleController.");
            Transform leftAnchor = offense ? offense.transform.Find("LeftHandBallAnchor") : null;
            Transform rightAnchor = offense ? offense.transform.Find("RightHandBallAnchor") : null;
            if (!leftAnchor) errors.Add("LeftHandBallAnchor is missing.");
            if (!rightAnchor) errors.Add("RightHandBallAnchor is missing.");

            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform contact = prototype ? prototype.Find("BallFloorContactIndicator") : null;
            if (!contact) errors.Add("BallFloorContactIndicator is missing.");

            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>())
                errors.Add("A CharacterController exists; Chunk 2 must not add locomotion.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>())
                errors.Add("A Rigidbody exists; the bounce must remain scripted.");

            if (errors.Count == 0)
            {
                Vector3 offensePosition = offense.transform.position;
                Vector3 defenderPosition = defender.transform.position;
                Vector3 cameraPosition = cameraObject.transform.position;
                Quaternion cameraRotation = cameraObject.transform.rotation;

                ValidateHandCycle(controller, BallHand.Left, leftAnchor, true, contact.gameObject, errors);
                ValidateHandCycle(controller, BallHand.Right, rightAnchor, false, contact.gameObject, errors);

                AssertStill("OffensivePlayer", offensePosition, offense.transform.position, errors);
                AssertStill("Defender", defenderPosition, defender.transform.position, errors);
                AssertStill("Main Camera", cameraPosition, cameraObject.transform.position, errors);
                if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > 0.001f)
                    errors.Add("Main Camera rotation changed during dribble validation.");
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 2 validation failed:\n- " + string.Join("\n- ", errors));

            Debug.Log("Bounce Theory Chunk 2 validation passed: both starting hands snap correctly; inactive-hand input is ignored; each isolated active input produces one scripted floor-contact cycle; player, defender, and camera remain stationary; no CharacterController or Rigidbody is present.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 3 Internal Rhythm")]
        public static void ValidateChunk3()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            RhythmClock clock = prototype ? prototype.Find("RhythmClock")?.GetComponent<RhythmClock>() : null;
            AudioSource audioSource = ball ? ball.GetComponent<AudioSource>() : null;

            if (!controller) errors.Add("Basketball has no PoundDribbleController.");
            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (controller && controller.RhythmClock != clock) errors.Add("PoundDribbleController is not using the scene RhythmClock.");
            if (!audioSource) errors.Add("Basketball has no prototype AudioSource.");
            if (!AssetDatabase.LoadAssetAtPath<AudioClip>(PrototypeBounceAudioPath)) errors.Add("Prototype floor-contact sound is missing.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController exists; Chunk 3 must not add locomotion.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody exists; Chunk 3 bounce motion must remain scripted.");

            if (clock)
            {
                ValidateJudgment(clock.JudgeAtElapsedTime(0.0), TimingResult.Perfect, TimingDirection.OnBeat, "on-beat", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(0.028), TimingResult.Good, TimingDirection.Late, "good-late", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(clock.SecondsPerBeat - 0.028), TimingResult.Good, TimingDirection.Early, "good-early", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(0.045), TimingResult.Late, TimingDirection.Late, "late", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(clock.SecondsPerBeat - 0.045), TimingResult.Early, TimingDirection.Early, "early", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(0.070), TimingResult.BrokenRhythm, TimingDirection.Late, "broken-late", errors);
                ValidateJudgment(clock.JudgeAtElapsedTime(clock.SecondsPerBeat - 0.070), TimingResult.BrokenRhythm, TimingDirection.Early, "broken-early", errors);
                ValidateFrameRateIndependence(clock, errors);
            }

            if (errors.Count == 0)
            {
                Vector3 offensePosition = offense.transform.position;
                Vector3 defenderPosition = defender.transform.position;
                Vector3 cameraPosition = cameraObject.transform.position;
                Quaternion cameraRotation = cameraObject.transform.rotation;
                ValidateTimedInputAndMotion(controller, clock, errors);
                AssertStill("OffensivePlayer", offensePosition, offense.transform.position, errors);
                AssertStill("Defender", defenderPosition, defender.transform.position, errors);
                AssertStill("Main Camera", cameraPosition, cameraObject.transform.position, errors);
                if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > 0.001f)
                    errors.Add("Main Camera rotation changed during Chunk 3 validation.");
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 3 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log("Bounce Theory Chunk 3 validation passed: time-based beat judgment covers Perfect, Good, Early, Late, and Broken Rhythm in both directions; results are frame-rate independent; valid isolated dribbles receive one judgment; silence/inactive-hand input are not judged; timing changes the scripted bounce; Chunk 1/2 transforms and ownership remain intact.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 4 Rhythmic Grid")]
        public static void ValidateChunk4()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform rhythmObject = prototype ? prototype.Find("RhythmClock") : null;
            RhythmClock clock = rhythmObject ? rhythmObject.GetComponent<RhythmClock>() : null;
            ReferenceGroovePlayer groove = rhythmObject ? rhythmObject.GetComponent<ReferenceGroovePlayer>() : null;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;

            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (!groove) errors.Add("ReferenceGroovePlayer is missing.");
            if (!controller) errors.Add("PoundDribbleController is missing.");
            if (clock && Mathf.Abs(clock.Bpm - 130f) > 0.001f) errors.Add("Chunk 4 default BPM is not 130.");
            if (groove && groove.RhythmClock != clock) errors.Add("Reference groove is not driven by the shared RhythmClock.");
            if (groove && !groove.ReferencePulse) errors.Add("Reference groove has no pulse clip.");
            if (!AssetDatabase.LoadAssetAtPath<AudioClip>(ReferencePulseAudioPath)) errors.Add("Generated reference pulse asset is missing.");
            if (groove && controller && groove.Volume >= controller.BounceVolume) errors.Add("Reference groove must be quieter than the ball impact.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController exists; Chunk 4 must not add locomotion.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody exists; Chunk 4 ball motion must remain scripted.");

            if (clock)
            {
                ValidateIntervalVocabulary(clock, errors);
                ValidateGridAlignmentAndAmbiguity(clock, errors);
                double a = clock.DspTimeForBeat(8.0);
                double b = clock.DspTimeForBeat(9.0);
                if (Math.Abs((b - a) - clock.SecondsPerBeat) > 0.000001)
                    errors.Add("Reference pulse beat scheduling is not using the clock's DSP-time beat spacing.");
            }

            if (controller && clock)
            {
                clock.ResetBallEventHistory();
                controller.ResetToStartingHand();
                controller.SetStartingHand(BallHand.Left);
                int before = controller.TimingJudgmentCount;
                if (controller.ProcessInputAtRhythmTime(false, false, 0)) errors.Add("Silence incorrectly started a dribble.");
                if (controller.TimingJudgmentCount != before) errors.Add("Silence created a timing judgment.");
                if (!controller.ProcessInputAtRhythmTime(true, false, 0)) errors.Add("W no longer starts a left-hand pound dribble.");
                if (controller.TimingJudgmentCount != before + 1) errors.Add("Accepted Chunk 4 dribble did not receive exactly one judgment.");
                CompleteCurrentDribble(controller, errors);
                controller.SetStartingHand(BallHand.Right);
                controller.ResetToStartingHand();
                clock.ResetBallEventHistory();
                if (!controller.ProcessInputAtRhythmTime(false, true, 0)) errors.Add("Up Arrow no longer starts a right-hand pound dribble.");
                CompleteCurrentDribble(controller, errors);
            }

            if (offense && defender && cameraObject)
            {
                AssertStill("OffensivePlayer", new Vector3(-.85f, 0, 1.1f), offense.transform.position, errors);
                AssertStill("Defender", new Vector3(.35f, 0, 8.15f), defender.transform.position, errors);
                AssertStill("Main Camera", new Vector3(0, 4.55f, -7.6f), cameraObject.transform.position, errors);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 4 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log("Bounce Theory Chunk 4 validation passed: 130 BPM DSP grid, all five inferred intervals, Easy timing directions/results, non-overlapping adjacent targets, drift-resistant quarter-beat alignment, quiet synchronized reference pulse, one judgment per accepted input, preserved two-hand controls, and stationary scene subjects.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 4.5 Input Authority")]
        public static void ValidateChunk45()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            RhythmClock clock = prototype ? prototype.Find("RhythmClock")?.GetComponent<RhythmClock>() : null;
            ReferenceGroovePlayer groove = prototype ? prototype.Find("RhythmClock")?.GetComponent<ReferenceGroovePlayer>() : null;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;

            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (!groove) errors.Add("Reference groove is missing.");
            if (!controller) errors.Add("PoundDribbleController is missing.");
            if (clock && Mathf.Abs(clock.Bpm - 130f) > .001f) errors.Add("BPM changed from the 130 default.");
            if (controller && Mathf.Abs(controller.HandHeight - 1.56f) > .001f) errors.Add("Readable prototype hand height is not 1.56.");
            if (UnityEngine.Object.FindObjectsByType<PoundDribbleController>(FindObjectsSortMode.None).Length != 1)
                errors.Add("The scene does not contain exactly one authoritative basketball controller.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController exists; locomotion remains out of scope.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody exists; ball motion must remain scripted.");

            if (controller && clock)
                ValidatePendingInputAuthority(controller, clock, errors);

            if (offense && defender && cameraObject)
            {
                AssertStill("OffensivePlayer", new Vector3(-.85f, 0, 1.1f), offense.transform.position, errors);
                AssertStill("Defender", new Vector3(.35f, 0, 8.15f), defender.transform.position, errors);
                AssertStill("Main Camera", new Vector3(0, 4.55f, -7.6f), cameraObject.transform.position, errors);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 4.5 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log("Bounce Theory Chunk 4.5 validation passed with target-contact scheduling: active-motion follow-up input retains its original DSP time and one pending slot, executes without rejudgment at the earliest feasible Returning phase, queue-full rejection is explicit, 0.5-beat inference survives visual overlap, ball height is 1.56, and the Chunk 4 grid/scene remain intact.");
        }

        [MenuItem("Bounce Theory/Validate Chunk 4.75 Target Contact")]
        public static void ValidateChunk475()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            Transform rhythmObject = prototype ? prototype.Find("RhythmClock") : null;
            RhythmClock clock = rhythmObject ? rhythmObject.GetComponent<RhythmClock>() : null;
            ReferenceGroovePlayer groove = rhythmObject ? rhythmObject.GetComponent<ReferenceGroovePlayer>() : null;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;

            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (!groove) errors.Add("Reference groove is missing.");
            if (!controller) errors.Add("PoundDribbleController is missing.");
            if (clock && Mathf.Abs(clock.Bpm - 130f) > .001f) errors.Add("BPM changed from 130.");
            if (groove && controller && groove.Volume >= controller.BounceVolume) errors.Add("Ball audio is not louder than the reference groove.");
            if (controller && Mathf.Abs(controller.HandHeight - 1.56f) > .001f) errors.Add("Normal readable hand height is not 1.56.");
            if (controller && (Mathf.Abs(controller.MinimumFastBounceHeight - 1.0f) > .001f ||
                               Mathf.Abs(controller.MaximumDescentSpeed - 8.0f) > .001f ||
                               Mathf.Abs(controller.TrajectoryCompressionAmount - .65f) > .001f ||
                               Mathf.Abs(controller.MinimumReadableContactApproachTime - .08f) > .001f))
                errors.Add("Adaptive motion defaults do not match the Chunk 4.75 tuning.");
            if (UnityEngine.Object.FindObjectsByType<PoundDribbleController>(FindObjectsSortMode.None).Length != 1)
                errors.Add("The scene does not contain exactly one basketball controller.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody was introduced.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController was introduced.");

            if (controller && clock) ValidateTargetContactScheduling(controller, clock, errors);

            if (offense && defender && cameraObject)
            {
                AssertStill("OffensivePlayer", new Vector3(-.85f, 0, 1.1f), offense.transform.position, errors);
                AssertStill("Defender", new Vector3(.35f, 0, 8.15f), defender.transform.position, errors);
                AssertStill("Main Camera", new Vector3(0, 4.55f, -7.6f), cameraObject.transform.position, errors);
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("Chunk 4.75 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log($"Bounce Theory Chunk 4.75 validation passed: normal and compressed descents reach explicit grid-aligned DSP contact targets; input/contact times are independent; 0.5-beat contact interrupts the visual return; missed targets reject explicitly; actual contact error was {controller.LastContactErrorMilliseconds:+0.00;-0.00;0.00} ms in the final scheduled-contact sample.");
        }

        [MenuItem("Bounce Theory/Validate BT-BC-06 Basic Crossover")]
        public static void ValidateBasicCrossover()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            RhythmClock clock = prototype ? prototype.Find("RhythmClock")?.GetComponent<RhythmClock>() : null;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            Transform leftAnchor = offense ? offense.transform.Find("LeftHandBallAnchor") : null;
            Transform rightAnchor = offense ? offense.transform.Find("RightHandBallAnchor") : null;
            Transform contact = prototype ? prototype.Find("BallFloorContactIndicator") : null;

            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (!controller) errors.Add("PoundDribbleController is missing.");
            if (controller && controller.RhythmClock != clock) errors.Add("Crossover does not use the shared RhythmClock.");
            if (!leftAnchor) errors.Add("LeftHandBallAnchor is missing.");
            if (!rightAnchor) errors.Add("RightHandBallAnchor is missing.");
            if (!contact) errors.Add("BallFloorContactIndicator is missing.");
            if (UnityEngine.Object.FindObjectsByType<PoundDribbleController>(FindObjectsSortMode.None).Length != 1)
                errors.Add("The scene does not contain exactly one authoritative basketball controller.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody was introduced.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController was introduced.");

            if (controller && clock && leftAnchor && rightAnchor && contact)
            {
                Vector3 offensePosition = offense.transform.position;
                Vector3 defenderPosition = defender.transform.position;
                Vector3 cameraPosition = cameraObject.transform.position;
                Quaternion cameraRotation = cameraObject.transform.rotation;

                ValidateHandCycle(controller, BallHand.Left, leftAnchor, true, contact.gameObject, errors);
                ValidateHandCycle(controller, BallHand.Right, rightAnchor, false, contact.gameObject, errors);
                ValidateCrossoverCycle(controller, clock, BallHand.Left, leftAnchor, rightAnchor, errors);
                ValidateCrossoverCycle(controller, clock, BallHand.Right, rightAnchor, leftAnchor, errors);
                ValidateCrossoverDuringReturn(controller, clock, errors);
                ValidatePendingInputAuthority(controller, clock, errors);
                ValidateTargetContactScheduling(controller, clock, errors);

                AssertStill("OffensivePlayer", offensePosition, offense.transform.position, errors);
                AssertStill("Defender", defenderPosition, defender.transform.position, errors);
                AssertStill("Main Camera", cameraPosition, cameraObject.transform.position, errors);
                if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > .001f)
                    errors.Add("Main Camera rotation changed during crossover validation.");
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("BT-BC-06 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log("BT-BC-06 validation passed: D crosses left-to-right, Left Arrow crosses right-to-left, ownership transfers at floor contact, the path travels laterally across the body, crossover follow-up can begin during a prior return, shared DSP contact planning is preserved, pound regressions pass, and scene subjects remain stationary.");
        }

        [MenuItem("Bounce Theory/Validate BT-BC-07 Basic Hesitation")]
        public static void ValidateBasicHesitation()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            GameObject offense = Required("OffensivePlayer", errors);
            GameObject defender = Required("Defender", errors);
            GameObject ball = Required("Basketball", errors);
            GameObject cameraObject = Required("Main Camera", errors);
            Transform prototype = GameObject.Find("BounceTheoryPrototype")?.transform;
            RhythmClock clock = prototype ? prototype.Find("RhythmClock")?.GetComponent<RhythmClock>() : null;
            PoundDribbleController controller = ball ? ball.GetComponent<PoundDribbleController>() : null;
            Transform leftAnchor = offense ? offense.transform.Find("LeftHandBallAnchor") : null;
            Transform rightAnchor = offense ? offense.transform.Find("RightHandBallAnchor") : null;
            Transform contact = prototype ? prototype.Find("BallFloorContactIndicator") : null;

            if (!clock) errors.Add("Reusable RhythmClock is missing.");
            if (!controller) errors.Add("PoundDribbleController is missing.");
            if (controller && controller.RhythmClock != clock) errors.Add("Hesitation does not use the shared RhythmClock.");
            if (!leftAnchor) errors.Add("LeftHandBallAnchor is missing.");
            if (!rightAnchor) errors.Add("RightHandBallAnchor is missing.");
            if (!contact) errors.Add("BallFloorContactIndicator is missing.");
            if (UnityEngine.Object.FindObjectsByType<PoundDribbleController>(FindObjectsSortMode.None).Length != 1)
                errors.Add("The scene does not contain exactly one authoritative basketball controller.");
            if (UnityEngine.Object.FindAnyObjectByType<Rigidbody>()) errors.Add("A Rigidbody was introduced.");
            if (UnityEngine.Object.FindAnyObjectByType<CharacterController>()) errors.Add("A CharacterController was introduced.");

            if (controller && clock && leftAnchor && rightAnchor && contact)
            {
                Vector3 offensePosition = offense.transform.position;
                Vector3 defenderPosition = defender.transform.position;
                Vector3 cameraPosition = cameraObject.transform.position;
                Quaternion cameraRotation = cameraObject.transform.rotation;

                ValidateHandCycle(controller, BallHand.Left, leftAnchor, true, contact.gameObject, errors);
                ValidateHandCycle(controller, BallHand.Right, rightAnchor, false, contact.gameObject, errors);
                ValidateCrossoverCycle(controller, clock, BallHand.Left, leftAnchor, rightAnchor, errors);
                ValidateCrossoverCycle(controller, clock, BallHand.Right, rightAnchor, leftAnchor, errors);
                ValidateHesitationCycle(controller, clock, BallHand.Left, leftAnchor, errors);
                ValidateHesitationCycle(controller, clock, BallHand.Right, rightAnchor, errors);
                ValidateHesitationFollowUp(controller, clock, errors);
                ValidateCrossoverDuringReturn(controller, clock, errors);
                ValidatePendingInputAuthority(controller, clock, errors);
                ValidateTargetContactScheduling(controller, clock, errors);

                AssertStill("OffensivePlayer", offensePosition, offense.transform.position, errors);
                AssertStill("Defender", defenderPosition, defender.transform.position, errors);
                AssertStill("Main Camera", cameraPosition, cameraObject.transform.position, errors);
                if (Quaternion.Angle(cameraRotation, cameraObject.transform.rotation) > .001f)
                    errors.Add("Main Camera rotation changed during hesitation validation.");
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("BT-BC-07 validation failed:\n- " + string.Join("\n- ", errors));
            Debug.Log("BT-BC-07 validation passed: A hesitates with the left hand, Right Arrow hesitates with the right hand, ownership remains unchanged, the near-hand lift/hold is distinct from a pound and crossover, hesitation receives shared DSP contact timing, a judged pound follow-up executes before hesitation visuals fully resolve, prior action regressions pass, and scene subjects remain stationary.");
        }

        public static void CapturePreview()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            UnityEngine.Camera camera = GameObject.Find("Main Camera")?.GetComponent<UnityEngine.Camera>();
            if (!camera) throw new InvalidOperationException("Main Camera was not found for preview capture.");

            string outputPath = Path.Combine(Path.GetTempPath(), "BounceTheoryPrototypePreview.png");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-bouncePreviewPath") outputPath = args[i + 1];

            const int width = 1280;
            const int height = 720;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var request = new RenderPipeline.StandardRequest { destination = renderTexture };
            if (RenderPipeline.SupportsRenderRequest(camera, request))
                RenderPipeline.SubmitRenderRequest(camera, request);
            else
            {
                camera.targetTexture = renderTexture;
                camera.Render();
                camera.targetTexture = null;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            File.WriteAllBytes(outputPath, image.EncodeToPNG());
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            renderTexture.Release();
            UnityEngine.Object.DestroyImmediate(renderTexture);
            Debug.Log("Bounce Theory preview captured: " + outputPath);
        }

        private static Dictionary<string, Material> CreateMaterials()
        {
            return new Dictionary<string, Material>
            {
                ["court"] = Mat("Court_Hardwood", new Color(0.58f, 0.285f, 0.085f), 0.18f, 0.4f),
                ["paint"] = Mat("Court_PaintedArea", new Color(0.08f, 0.16f, 0.23f), 0.05f, 0.35f),
                ["line"] = Mat("Court_Lines", new Color(0.97f, 0.92f, 0.76f), 0f, 0.35f, true),
                ["surround"] = Mat("Court_Surround", new Color(0.025f, 0.04f, 0.065f), 0f, 0.25f),
                ["blue"] = Mat("Offense_Blue", new Color(0.03f, 0.52f, 0.95f), 0f, 0.2f, true),
                ["navy"] = Mat("Offense_Navy", new Color(0.015f, 0.075f, 0.18f), 0f, 0.18f, true),
                ["red"] = Mat("Defender_Red", new Color(0.95f, 0.12f, 0.13f), 0f, 0.2f, true),
                ["maroon"] = Mat("Defender_Maroon", new Color(0.26f, 0.015f, 0.025f), 0f, 0.18f, true),
                ["skin"] = Mat("Placeholder_Skin", new Color(0.52f, 0.29f, 0.16f), 0f, 0.4f, true),
                ["ball"] = Mat("Basketball_Orange", new Color(1f, 0.29f, 0.015f), 0f, 0.16f, true),
                ["seam"] = Mat("Basketball_Seams", new Color(0.035f, 0.025f, 0.02f), 0f, 0.25f, true),
                ["leftHand"] = Mat("HandAnchor_Left", new Color(0.05f, 0.9f, 1f), 0f, 0.2f, true),
                ["rightHand"] = Mat("HandAnchor_Right", new Color(1f, 0.82f, 0.05f), 0f, 0.2f, true),
                ["contact"] = Mat("FloorContact_Debug", new Color(0.72f, 1f, 0.05f), 0f, 0.1f, true),
                ["board"] = Mat("Backboard_White", new Color(0.9f, 0.95f, 1f), 0f, 0.7f, true),
                ["metal"] = Mat("Basket_Metal", new Color(0.12f, 0.14f, 0.17f), 0.55f, 0.55f),
                ["rim"] = Mat("Basket_Rim", new Color(1f, 0.18f, 0.015f), 0.15f, 0.55f, true),
                ["net"] = Mat("Basket_Net", new Color(0.94f, 0.97f, 1f), 0f, 0.35f, true)
            };
        }

        private static Material Mat(string name, Color color, float metallic, float smoothness, bool unlit = false)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (!shader) throw new InvalidOperationException("No compatible lit shader found.");
            if (!material)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else material.shader = shader;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Court(Transform parent, IReadOnlyDictionary<string, Material> m)
        {
            Transform env = Group("Environment", parent);
            Primitive(PrimitiveType.Cube, "Surround", new Vector3(0, -0.24f, 7), new Vector3(28, 0.28f, 34), m["surround"], env);
            Primitive(PrimitiveType.Cube, "HalfCourt", new Vector3(0, -0.08f, 7), new Vector3(14, 0.2f, 22), m["court"], env);
            Primitive(PrimitiveType.Cube, "PaintedKey", new Vector3(0, 0.025f, 14.5f), new Vector3(4.8f, 0.035f, 6.9f), m["paint"], env);
            Transform lines = Group("CourtLines", env);
            Line("LeftSideline", new Vector3(-6.9f, .14f, -3.7f), new Vector3(-6.9f, .14f, 17.62f), .1f, m["line"], lines);
            Line("RightSideline", new Vector3(6.9f, .14f, -3.7f), new Vector3(6.9f, .14f, 17.62f), .1f, m["line"], lines);
            Line("HalfCourtLine", new Vector3(-6.9f, .14f, -3.65f), new Vector3(6.9f, .14f, -3.65f), .1f, m["line"], lines);
            Line("Baseline", new Vector3(-6.9f, .14f, 17.62f), new Vector3(6.9f, .14f, 17.62f), .1f, m["line"], lines);
            Line("KeyLeft", new Vector3(-2.4f, .14f, 11.05f), new Vector3(-2.4f, .14f, 17.58f), .085f, m["line"], lines);
            Line("KeyRight", new Vector3(2.4f, .14f, 11.05f), new Vector3(2.4f, .14f, 17.58f), .085f, m["line"], lines);
            Line("FreeThrowLine", new Vector3(-2.4f, .14f, 11.05f), new Vector3(2.4f, .14f, 11.05f), .085f, m["line"], lines);
            Arc("FreeThrowArc", new Vector3(0, .14f, 11.05f), 1.78f, 0, 360, 24, .075f, m["line"], lines, false);
            Arc("ThreePointArc", new Vector3(0, .14f, 16.78f), 6.2f, -67, 67, 26, .1f, m["line"], lines, true);
            Line("ThreePointLeftWing", new Vector3(-5.7f, .14f, 14.35f), new Vector3(-5.7f, .14f, 17.58f), .1f, m["line"], lines);
            Line("ThreePointRightWing", new Vector3(5.7f, .14f, 14.35f), new Vector3(5.7f, .14f, 17.58f), .1f, m["line"], lines);
        }

        private static void Player(Transform parent, IReadOnlyDictionary<string, Material> m)
        {
            Transform root = Group("OffensivePlayer", parent);
            root.position = new Vector3(-.85f, 0, 1.1f);
            Primitive(PrimitiveType.Cylinder, "OffenseMarker", new Vector3(0, .125f, 0), new Vector3(1, .025f, 1), m["navy"], root);
            Primitive(PrimitiveType.Capsule, "Torso", new Vector3(0, 1.45f, 0), new Vector3(.82f, .9f, .62f), m["blue"], root);
            Primitive(PrimitiveType.Sphere, "Head", new Vector3(0, 2.7f, 0), Vector3.one * .68f, m["skin"], root);
            Primitive(PrimitiveType.Capsule, "LeftLeg", new Vector3(-.28f, .62f, .02f), new Vector3(.28f, .63f, .28f), m["navy"], root, new Vector3(0, 0, 8));
            Primitive(PrimitiveType.Capsule, "RightLeg", new Vector3(.28f, .62f, .02f), new Vector3(.28f, .63f, .28f), m["navy"], root, new Vector3(0, 0, -8));
            Primitive(PrimitiveType.Capsule, "LeftArm", new Vector3(-.63f, 1.62f, .1f), new Vector3(.22f, .62f, .22f), m["skin"], root, new Vector3(12, 0, -32));
            Primitive(PrimitiveType.Capsule, "RightArm", new Vector3(.63f, 1.62f, .1f), new Vector3(.22f, .62f, .22f), m["skin"], root, new Vector3(12, 0, 32));
            Primitive(PrimitiveType.Cube, "JerseyStripe", new Vector3(0, 1.55f, -.33f), new Vector3(.16f, 1.1f, .035f), m["line"], root);
        }

        private static void Ball(Transform parent, IReadOnlyDictionary<string, Material> m)
        {
            Transform root = Group("Basketball", parent);
            root.position = new Vector3(.32f, .47f, 1.25f);
            Primitive(PrimitiveType.Sphere, "Ball", Vector3.zero, Vector3.one * .72f, m["ball"], root);
            Primitive(PrimitiveType.Cube, "HorizontalSeam", new Vector3(0, 0, -.355f), new Vector3(.72f, .035f, .02f), m["seam"], root);
            Primitive(PrimitiveType.Cube, "VerticalSeam", new Vector3(0, 0, -.357f), new Vector3(.035f, .72f, .02f), m["seam"], root);
        }

        private static AudioClip CreatePrototypeBounceAudio()
        {
            EnsureFolder("Assets/BounceTheory", "Audio");
            if (!File.Exists(PrototypeBounceAudioPath))
            {
                const int sampleRate = 44100;
                const float duration = 0.12f;
                int sampleCount = Mathf.CeilToInt(sampleRate * duration);
                int dataSize = sampleCount * sizeof(short);
                using (var memory = new MemoryStream(44 + dataSize))
                using (var writer = new BinaryWriter(memory))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write(36 + dataSize);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                    writer.Write(16);
                    writer.Write((short)1);
                    writer.Write((short)1);
                    writer.Write(sampleRate);
                    writer.Write(sampleRate * sizeof(short));
                    writer.Write((short)sizeof(short));
                    writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                    writer.Write(dataSize);

                    var noise = new System.Random(3721);
                    for (int i = 0; i < sampleCount; i++)
                    {
                        double t = i / (double)sampleRate;
                        double envelope = Math.Exp(-34.0 * t);
                        double lowThump = Math.Sin(2.0 * Math.PI * (92.0 - 180.0 * t) * t) * 0.78;
                        double texture = (noise.NextDouble() * 2.0 - 1.0) * 0.12;
                        double sample = Math.Max(-1.0, Math.Min(1.0, (lowThump + texture) * envelope));
                        writer.Write((short)Math.Round(sample * short.MaxValue));
                    }
                    File.WriteAllBytes(PrototypeBounceAudioPath, memory.ToArray());
                }
            }

            AssetDatabase.ImportAsset(PrototypeBounceAudioPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(PrototypeBounceAudioPath);
            if (!clip) throw new InvalidOperationException("Could not import generated prototype bounce sound.");
            return clip;
        }

        private static AudioClip CreateReferencePulseAudio()
        {
            EnsureFolder("Assets/BounceTheory", "Audio");
            if (!File.Exists(ReferencePulseAudioPath))
            {
                const int sampleRate = 44100;
                const float duration = 0.045f;
                int sampleCount = Mathf.CeilToInt(sampleRate * duration);
                int dataSize = sampleCount * sizeof(short);
                using (var memory = new MemoryStream(44 + dataSize))
                using (var writer = new BinaryWriter(memory))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                    writer.Write(36 + dataSize);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                    writer.Write(16);
                    writer.Write((short)1);
                    writer.Write((short)1);
                    writer.Write(sampleRate);
                    writer.Write(sampleRate * sizeof(short));
                    writer.Write((short)sizeof(short));
                    writer.Write((short)16);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                    writer.Write(dataSize);

                    for (int i = 0; i < sampleCount; i++)
                    {
                        double t = i / (double)sampleRate;
                        double envelope = Math.Exp(-95.0 * t);
                        double click = Math.Sin(2.0 * Math.PI * 760.0 * t) * 0.42;
                        writer.Write((short)Math.Round(click * envelope * short.MaxValue));
                    }
                    File.WriteAllBytes(ReferencePulseAudioPath, memory.ToArray());
                }
            }

            AssetDatabase.ImportAsset(ReferencePulseAudioPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ReferencePulseAudioPath);
            if (!clip) throw new InvalidOperationException("Could not import generated reference pulse.");
            return clip;
        }

        private static void ValidateIntervalVocabulary(RhythmClock clock, ICollection<string> errors)
        {
            clock.ResetBallEventHistory();
            clock.RegisterBallEventAtElapsedTime(4.0 * clock.SecondsPerBeat);
            foreach (RhythmicIntervalKind interval in RhythmicIntervalCatalog.Ordered)
            {
                double targetBeat = 4.0 + RhythmicIntervalCatalog.Beats(interval);
                TimingJudgment judgment = clock.JudgeNextIntervalAtElapsedTime(targetBeat * clock.SecondsPerBeat);
                if (judgment.Interval != interval) errors.Add("Failed to infer " + RhythmicIntervalCatalog.Label(interval) + ".");
                if (judgment.Result != TimingResult.Perfect) errors.Add(RhythmicIntervalCatalog.Label(interval) + " exact target was not Perfect.");
            }

            double oneBeatTarget = 5.0 * clock.SecondsPerBeat;
            ValidateJudgment(clock.JudgeNextIntervalAtElapsedTime(oneBeatTarget - .010), TimingResult.Perfect, TimingDirection.Early, "interval perfect-early", errors);
            ValidateJudgment(clock.JudgeNextIntervalAtElapsedTime(oneBeatTarget + .028), TimingResult.Good, TimingDirection.Late, "interval good-late", errors);
            ValidateJudgment(clock.JudgeNextIntervalAtElapsedTime(oneBeatTarget - .045), TimingResult.Early, TimingDirection.Early, "interval early", errors);
            ValidateJudgment(clock.JudgeNextIntervalAtElapsedTime(oneBeatTarget + .045), TimingResult.Late, TimingDirection.Late, "interval late", errors);
            ValidateJudgment(clock.JudgeNextIntervalAtElapsedTime(oneBeatTarget + .070), TimingResult.BrokenRhythm, TimingDirection.Late, "interval broken", errors);
        }

        private static void ValidateGridAlignmentAndAmbiguity(RhythmClock clock, ICollection<string> errors)
        {
            double quarterSpacingMs = clock.SecondsPerBeat * 250.0;
            if (clock.EffectiveMaximumIntervalWindowMilliseconds * 2.0 >= quarterSpacingMs)
                errors.Add("Easy maximum windows can overlap between adjacent interval targets.");

            clock.ResetBallEventHistory();
            clock.RegisterBallEventAtElapsedTime(4.0 * clock.SecondsPerBeat + .020);
            if (Math.Abs(clock.PreviousBallEventAlignedBeat - 4.0) > 0.000001)
                errors.Add("Previous floor event was not aligned to the global subdivision grid.");
            TimingJudgment stable = clock.JudgeNextIntervalAtElapsedTime(5.0 * clock.SecondsPerBeat);
            if (stable.Interval != RhythmicIntervalKind.OneBeat || stable.Result != TimingResult.Perfect)
                errors.Add("A small prior event error accumulated into the next interval target.");

            double midpointBeat = 4.625;
            TimingJudgment midpoint = clock.JudgeNextIntervalAtElapsedTime(midpointBeat * clock.SecondsPerBeat);
            if (midpoint.Result != TimingResult.BrokenRhythm)
                errors.Add("Midpoint between adjacent 0.5 and 0.75 beat targets was not an unambiguous Broken Rhythm gap.");
        }

        private static void ValidateJudgment(TimingJudgment judgment, TimingResult expectedResult, TimingDirection expectedDirection, string label, ICollection<string> errors)
        {
            if (judgment.Result != expectedResult)
                errors.Add(label + " expected " + expectedResult + " but received " + judgment.Result + ".");
            if (judgment.Direction != expectedDirection)
                errors.Add(label + " expected " + expectedDirection + " direction but received " + judgment.Direction + ".");
        }

        private static void ValidateFrameRateIndependence(RhythmClock clock, ICollection<string> errors)
        {
            int[] frameRates = { 30, 60, 120, 144 };
            TimingJudgment? baseline = null;
            foreach (int frameRate in frameRates)
            {
                double elapsed = 0.0;
                int frames = (int)Math.Round(1.5 * frameRate);
                for (int i = 0; i < frames; i++) elapsed += 1.0 / frameRate;
                TimingJudgment judgment = clock.JudgeAtElapsedTime(elapsed);
                if (!baseline.HasValue) baseline = judgment;
                else if (judgment.Result != baseline.Value.Result || judgment.Direction != baseline.Value.Direction ||
                         Math.Abs(judgment.ErrorMilliseconds - baseline.Value.ErrorMilliseconds) > 0.01)
                    errors.Add("Timing judgment changed at simulated " + frameRate + " FPS.");
            }
        }

        private static void ValidateTimedInputAndMotion(PoundDribbleController controller, RhythmClock clock, ICollection<string> errors)
        {
            clock.ResetBallEventHistory();
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Left);
            int initialJudgments = controller.TimingJudgmentCount;
            double idealInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            if (controller.ProcessInputAtRhythmTime(false, true, idealInput)) errors.Add("Inactive right-hand input was accepted while Left owned the ball.");
            if (controller.ProcessInputAtRhythmTime(false, false, idealInput)) errors.Add("No-input state incorrectly started or judged a dribble.");
            if (controller.TimingJudgmentCount != initialJudgments) errors.Add("Inactive/no input created a timing judgment.");

            if (!controller.ProcessInputAtRhythmTime(true, false, idealInput)) errors.Add("Left W path did not accept an on-beat dribble.");
            if (controller.TimingJudgmentCount != initialJudgments + 1) errors.Add("Accepted dribble did not receive exactly one timing judgment.");
            if (controller.LastTimingJudgment.Result != TimingResult.Perfect) errors.Add("On-beat accepted dribble was not Perfect.");
            float perfectDuration = controller.ActiveBounceDuration;
            CompleteCurrentDribble(controller, errors);

            clock.ResetBallEventHistory();
            controller.ResetToStartingHand();
            double earlyTime = idealInput - 0.045;
            controller.ProcessInputAtRhythmTime(true, false, earlyTime);
            float earlyDuration = controller.ActiveBounceDuration;
            if (controller.LastTimingJudgment.Result != TimingResult.Early) errors.Add("Clearly early accepted dribble was not Early.");
            CompleteCurrentDribble(controller, errors);

            clock.ResetBallEventHistory();
            controller.ResetToStartingHand();
            controller.ProcessInputAtRhythmTime(true, false, idealInput + 0.045);
            float lateDuration = controller.ActiveBounceDuration;
            if (controller.LastTimingJudgment.Result != TimingResult.Late) errors.Add("Clearly late accepted dribble was not Late.");
            CompleteCurrentDribble(controller, errors);

            clock.ResetBallEventHistory();
            controller.ResetToStartingHand();
            controller.ProcessInputAtRhythmTime(true, false, idealInput + 0.070);
            float brokenDuration = controller.ActiveBounceDuration;
            if (controller.LastTimingJudgment.Result != TimingResult.BrokenRhythm) errors.Add("Large-error accepted dribble was not Broken Rhythm.");
            CompleteCurrentDribble(controller, errors);

            if (!(earlyDuration < perfectDuration)) errors.Add("Early timing did not create the configured rushed bounce.");
            if (!(lateDuration > perfectDuration)) errors.Add("Late timing did not create the configured heavier bounce.");
            if (!(brokenDuration > lateDuration)) errors.Add("Broken Rhythm did not create the least-controlled duration.");

            clock.ResetBallEventHistory();
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Right);
            int beforeRight = controller.TimingJudgmentCount;
            if (controller.ProcessInputAtRhythmTime(true, false, idealInput)) errors.Add("W was accepted while Right owned the ball.");
            if (!controller.ProcessInputAtRhythmTime(false, true, idealInput)) errors.Add("Right Up Arrow path did not accept an on-beat dribble.");
            if (controller.TimingJudgmentCount != beforeRight + 1) errors.Add("Right-hand accepted dribble did not receive exactly one judgment.");
            CompleteCurrentDribble(controller, errors);
        }

        private static void ValidatePendingInputAuthority(PoundDribbleController controller, RhythmClock clock, ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Left);
            clock.ResetBallEventHistory();
            int judgmentsBefore = controller.TimingJudgmentCount;
            int completedBefore = controller.CompletedDribbleCount;

            double firstInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            if (!controller.ProcessInputAtRhythmTime(true, false, firstInput))
                errors.Add("Initial left-hand pound was not accepted.");
            double halfBeatTarget = controller.ActiveContactPlan.TargetContactElapsedSeconds + .5 * clock.SecondsPerBeat;
            double halfBeatInput = halfBeatTarget - controller.ReferenceDescentDurationSeconds;
            if (!controller.ProcessInputAtRhythmTime(true, false, halfBeatInput))
                errors.Add("Valid 0.5-beat follow-up was not accepted during active visual motion.");
            if (!controller.HasPendingInput) errors.Add("Accepted active-motion follow-up was not stored in the pending slot.");
            TimingJudgment preserved = controller.PendingTimingJudgment;
            double preservedDsp = controller.PendingInputDspTimestamp;
            if (preserved.Interval != RhythmicIntervalKind.HalfBeat)
                errors.Add("Fast follow-up did not retain its inferred 0.5-beat interval.");
            if (preserved.Result != TimingResult.Perfect)
                errors.Add("Exact 0.5-beat follow-up was not judged Perfect at keypress time.");
            if (Math.Abs(preservedDsp - (clock.StartDspTime + halfBeatInput)) > .000001)
                errors.Add("Pending input did not retain its original DSP timestamp.");

            double occupiedAttempt = halfBeatInput + .010;
            if (controller.ProcessInputAtRhythmTime(true, false, occupiedAttempt))
                errors.Add("A second follow-up was accepted while the single pending slot was occupied.");
            if (!controller.LastInputDecision.Contains("occupied"))
                errors.Add("Queue-full input did not expose an explicit rejection reason.");
            if (!controller.HasPendingInput || controller.PendingTimingJudgment.Interval != preserved.Interval ||
                Math.Abs(controller.PendingInputDspTimestamp - preservedDsp) > .000001)
                errors.Add("Queue-full rejection overwrote the already pending input.");

            int judgmentsAfterInputDecisions = controller.TimingJudgmentCount;
            for (int i = 0; i < 500 && (controller.CompletedDribbleCount == completedBefore || controller.HasPendingInput); i++)
                controller.Tick(.005f);
            if (controller.CompletedDribbleCount != completedBefore + 1)
                errors.Add("First scheduled floor contact did not occur before pending execution.");
            if (controller.HasPendingInput)
                errors.Add("Pending slot was not consumed at the earliest feasible Returning phase.");
            if (controller.LogicalPhase != BallLogicalPhase.Descending)
                errors.Add("Pending pound did not begin adaptively before the prior return animation completed.");
            if (controller.ActiveActionJudgment.Result != preserved.Result ||
                controller.ActiveActionJudgment.Interval != preserved.Interval ||
                Math.Abs(controller.ActiveActionJudgment.ErrorMilliseconds - preserved.ErrorMilliseconds) > .000001)
                errors.Add("Pending action did not execute with its original timing judgment.");
            if (controller.TimingJudgmentCount != judgmentsAfterInputDecisions)
                errors.Add("Pending action was rejudged when visual execution began.");

            CompleteCurrentDribble(controller, errors);
            if (controller.CompletedDribbleCount != completedBefore + 2)
                errors.Add("Initial and pending inputs did not produce exactly two sequential dribbles.");
            if (controller.TimingJudgmentCount != judgmentsBefore + 3)
                errors.Add("Valid immediate, valid pending, and queue-full active inputs were not each judged exactly once.");
        }

        private static void ValidateTargetContactScheduling(PoundDribbleController controller, RhythmClock clock, ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Left);
            clock.ResetBallEventHistory();
            int completedBefore = controller.CompletedDribbleCount;
            int contactEvents = 0;
            controller.FloorContactReached += _ => contactEvents++;

            double normalInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            if (!controller.ProcessInputAtRhythmTime(true, false, normalInput))
                errors.Add("Normal one-beat contact request was not accepted.");
            ContactTimingPlan normalPlan = controller.ActiveContactPlan;
            if (normalPlan.TargetContactDspTimestamp <= normalPlan.InputDspTimestamp)
                errors.Add("Normal action has no future target floor-contact DSP timestamp.");
            if (controller.MotionMode != BounceMotionMode.Normal)
                errors.Add("Readable one-beat pound did not use Normal motion.");
            if (Math.Abs(normalPlan.TargetContactBeat - Math.Round(normalPlan.TargetContactBeat)) > .000001)
                errors.Add("Initial target contact is not aligned to a whole global beat.");

            for (int i = 0; i < 300 && controller.CompletedDribbleCount == completedBefore; i++) controller.Tick(1f / 240f);
            if (controller.CompletedDribbleCount != completedBefore + 1) errors.Add("Normal scheduled floor contact did not occur.");
            if (contactEvents != 1) errors.Add("Normal action did not emit exactly one FloorContact event.");
            if (Math.Abs(controller.LastActualFloorContactDsp - controller.LastTargetFloorContactDsp) > .006)
                errors.Add("Normal floor contact exceeded the 6 ms validation tolerance.");
            if (Math.Abs(controller.LastActualFloorContactDsp - normalPlan.InputDspTimestamp) < .05)
                errors.Add("Actual contact time was not independent from keypress time.");

            while (controller.LogicalPhase == BallLogicalPhase.FloorContact) controller.Tick(.005f);
            if (controller.LogicalPhase != BallLogicalPhase.Returning)
                errors.Add("Ball did not enter Returning after normal floor contact.");
            double fastInput = clock.ElapsedSecondsAtDspTime(controller.LastActualFloorContactDsp) + .085;
            if (!controller.ProcessInputAtRhythmTime(true, false, fastInput))
                errors.Add("Upcoming 0.5-beat target was blocked during the prior visual return.");
            ContactTimingPlan fastPlan = controller.ActiveContactPlan;
            if (fastPlan.Interval != RhythmicIntervalKind.HalfBeat)
                errors.Add("Returning-phase fast request did not select the 0.5-beat contact target.");
            if (controller.MotionMode != BounceMotionMode.Compressed)
                errors.Add("Fast 0.5-beat target did not use Compressed motion.");
            if (controller.LogicalPhase != BallLogicalPhase.Descending)
                errors.Add("Fast scheduled contact waited for the prior return animation to finish.");

            for (int i = 0; i < 300 && controller.CompletedDribbleCount == completedBefore + 1; i++) controller.Tick(1f / 240f);
            if (controller.CompletedDribbleCount != completedBefore + 2) errors.Add("Compressed 0.5-beat floor contact did not occur.");
            if (contactEvents != 2) errors.Add("Compressed action did not emit exactly one additional FloorContact event.");
            if (Math.Abs(controller.LastContactErrorMilliseconds) > 6.0)
                errors.Add("Compressed target contact error exceeded 6 ms.");
            if (Math.Abs(clock.PreviousBallEventAlignedBeat - fastPlan.TargetContactBeat) > .000001)
                errors.Add("Small measured contact error moved the global rhythm grid.");

            CompleteCurrentDribble(controller, errors);
            double missedInput = (clock.PreviousBallEventAlignedBeat + 2.5) * clock.SecondsPerBeat;
            if (controller.ProcessInputAtRhythmTime(true, false, missedInput))
                errors.Add("An already-missed target was silently accepted or moved to another beat.");
            if (controller.MotionMode != BounceMotionMode.Unreachable ||
                !(controller.LastInputDecision.Contains("passed") || controller.LastInputDecision.Contains("unreachable")))
                errors.Add("Missed physical target did not report an explicit Unreachable failure.");
        }

        private static void ValidateCrossoverCycle(PoundDribbleController controller, RhythmClock clock, BallHand sourceHand,
            Transform sourceAnchor, Transform targetAnchor, ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(sourceHand);
            clock.ResetBallEventHistory();
            int judgmentsBefore = controller.TimingJudgmentCount;
            int dribblesBefore = controller.CompletedDribbleCount;
            int crossoversBefore = controller.CompletedCrossoverCount;
            bool sourceIsLeft = sourceHand == BallHand.Left;

            bool inactiveAccepted = sourceIsLeft
                ? controller.ProcessCrossoverInputAtRhythmTime(false, true, 0)
                : controller.ProcessCrossoverInputAtRhythmTime(true, false, 0);
            if (inactiveAccepted) errors.Add(sourceHand + " ownership accepted crossover input from the inactive hand.");
            if (controller.TimingJudgmentCount != judgmentsBefore)
                errors.Add(sourceHand + " inactive crossover input created a timing judgment.");

            double idealInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            bool accepted = sourceIsLeft
                ? controller.ProcessCrossoverInputAtRhythmTime(true, false, idealInput)
                : controller.ProcessCrossoverInputAtRhythmTime(false, true, idealInput);
            if (!accepted) errors.Add(sourceHand + " crossover input was not accepted.");
            if (controller.ActiveAction != DribbleAction.Crossover)
                errors.Add(sourceHand + " crossover did not enter the shared action/contact path.");
            if (controller.ActiveContactPlan.TargetContactDspTimestamp <= controller.ActiveContactPlan.InputDspTimestamp)
                errors.Add(sourceHand + " crossover has no future DSP floor-contact target.");

            bool sawFloorContact = false;
            Vector3 contactPosition = sourceAnchor.position;
            BallHand expectedHand = sourceIsLeft ? BallHand.Right : BallHand.Left;
            for (int i = 0; i < 1000 && controller.IsDribbling; i++)
            {
                controller.Tick(.005f);
                if (controller.LogicalPhase == BallLogicalPhase.FloorContact)
                {
                    sawFloorContact = true;
                    contactPosition = controller.transform.position;
                    if (controller.CurrentHand != expectedHand)
                        errors.Add(sourceHand + " crossover did not transfer ownership at floor contact.");
                }
            }

            if (controller.IsDribbling) errors.Add(sourceHand + " crossover did not complete.");
            if (!sawFloorContact) errors.Add(sourceHand + " crossover never reached floor contact.");
            if (Mathf.Abs(contactPosition.x - sourceAnchor.position.x) < .25f)
                errors.Add(sourceHand + " crossover floor contact did not move laterally across the body.");
            if (controller.CurrentHand != expectedHand)
                errors.Add(sourceHand + " crossover did not finish with opposite-hand ownership.");
            if (Vector3.Distance(controller.transform.position, targetAnchor.position) > .001f)
                errors.Add(sourceHand + " crossover did not finish at the opposite-hand anchor.");
            if (controller.CompletedDribbleCount != dribblesBefore + 1 || controller.CompletedCrossoverCount != crossoversBefore + 1)
                errors.Add(sourceHand + " crossover did not register exactly one completed crossover contact.");
            if (controller.TimingJudgmentCount != judgmentsBefore + 1)
                errors.Add(sourceHand + " crossover did not receive exactly one timing judgment.");
        }

        private static void ValidateCrossoverDuringReturn(PoundDribbleController controller, RhythmClock clock,
            ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Left);
            clock.ResetBallEventHistory();
            double firstInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            if (!controller.ProcessInputAtRhythmTime(true, false, firstInput))
            {
                errors.Add("Return-overlap setup pound was not accepted.");
                return;
            }

            for (int i = 0; i < 500 && controller.LogicalPhase != BallLogicalPhase.Returning; i++) controller.Tick(.005f);
            if (controller.LogicalPhase != BallLogicalPhase.Returning)
            {
                errors.Add("Return-overlap setup did not reach Returning.");
                return;
            }

            const float returnOverlapSeconds = .20f;
            controller.Tick(returnOverlapSeconds);
            if (controller.LogicalPhase != BallLogicalPhase.Returning)
            {
                errors.Add("Return-overlap setup completed before the crossover input was tested.");
                return;
            }

            double overlapInput = clock.ElapsedSecondsAtDspTime(controller.LastActualFloorContactDsp) + returnOverlapSeconds;
            if (!controller.ProcessCrossoverInputAtRhythmTime(true, false, overlapInput))
                errors.Add("Rhythmically valid crossover was blocked during the prior visual return.");
            if (controller.ActiveAction != DribbleAction.Crossover || controller.LogicalPhase != BallLogicalPhase.Descending)
                errors.Add("Crossover follow-up waited for the prior return animation to complete.");
            CompleteCurrentDribble(controller, errors);
            if (controller.CurrentHand != BallHand.Right)
                errors.Add("Return-overlap crossover did not resolve to the right hand.");
        }

        private static void ValidateHesitationCycle(PoundDribbleController controller, RhythmClock clock,
            BallHand hand, Transform expectedAnchor, ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(hand);
            clock.ResetBallEventHistory();
            int judgmentsBefore = controller.TimingJudgmentCount;
            int dribblesBefore = controller.CompletedDribbleCount;
            int crossoversBefore = controller.CompletedCrossoverCount;
            bool handIsLeft = hand == BallHand.Left;

            bool inactiveAccepted = handIsLeft
                ? controller.ProcessHesitationInputAtRhythmTime(false, true, 0)
                : controller.ProcessHesitationInputAtRhythmTime(true, false, 0);
            if (inactiveAccepted) errors.Add(hand + " ownership accepted hesitation input from the inactive hand.");
            if (controller.TimingJudgmentCount != judgmentsBefore)
                errors.Add(hand + " inactive hesitation input created a timing judgment.");

            double idealInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            bool accepted = handIsLeft
                ? controller.ProcessHesitationInputAtRhythmTime(true, false, idealInput)
                : controller.ProcessHesitationInputAtRhythmTime(false, true, idealInput);
            if (!accepted) errors.Add(hand + " hesitation input was not accepted.");
            if (controller.ActiveAction != DribbleAction.Hesitation)
                errors.Add(hand + " hesitation did not enter the shared action/contact path.");
            if (controller.ActiveContactPlan.TargetContactDspTimestamp <= controller.ActiveContactPlan.InputDspTimestamp)
                errors.Add(hand + " hesitation has no future DSP floor-contact target.");

            float holdSampleSeconds = controller.ReferenceDescentDurationSeconds * controller.HesitationHoldFraction * .75f;
            controller.Tick(holdSampleSeconds);
            if (controller.transform.position.y < expectedAnchor.position.y + controller.HesitationLift * .75f)
                errors.Add(hand + " hesitation did not create a readable near-hand lift/hold distinct from a pound.");

            CompleteCurrentDribble(controller, errors);
            if (controller.CurrentHand != hand)
                errors.Add(hand + " hesitation changed logical hand ownership.");
            if (Vector3.Distance(controller.transform.position, expectedAnchor.position) > .001f)
                errors.Add(hand + " hesitation did not return to the same-hand anchor.");
            if (controller.CompletedDribbleCount != dribblesBefore + 1)
                errors.Add(hand + " hesitation did not register exactly one completed contact.");
            if (controller.CompletedCrossoverCount != crossoversBefore)
                errors.Add(hand + " hesitation incorrectly registered a crossover.");
            if (controller.TimingJudgmentCount != judgmentsBefore + 1)
                errors.Add(hand + " hesitation was not treated as one intentional judged action.");
        }

        private static void ValidateHesitationFollowUp(PoundDribbleController controller, RhythmClock clock,
            ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(BallHand.Left);
            clock.ResetBallEventHistory();
            int completedBefore = controller.CompletedDribbleCount;
            double firstInput = clock.SecondsPerBeat - controller.ReferenceDescentDurationSeconds;
            if (!controller.ProcessHesitationInputAtRhythmTime(true, false, firstInput))
            {
                errors.Add("Follow-up setup hesitation was not accepted.");
                return;
            }

            double followUpInput = firstInput + .10;
            if (!controller.ProcessInputAtRhythmTime(true, false, followUpInput))
                errors.Add("A rhythmically valid pound follow-up was not accepted during hesitation motion.");
            if (!controller.HasPendingInput || controller.PendingAction != DribbleAction.Pound)
                errors.Add("Hesitation follow-up did not retain its action in the pending input slot.");

            for (int i = 0; i < 500 && (controller.CompletedDribbleCount == completedBefore || controller.HasPendingInput); i++)
                controller.Tick(.005f);
            if (controller.CompletedDribbleCount != completedBefore + 1)
                errors.Add("Hesitation did not reach its scheduled contact before follow-up execution.");
            if (controller.HasPendingInput)
                errors.Add("Hesitation follow-up was not consumed at the earliest feasible phase.");
            if (controller.ActiveAction != DribbleAction.Pound || controller.LogicalPhase != BallLogicalPhase.Descending)
                errors.Add("Pound follow-up waited for the hesitation visual return to fully finish.");
            if (controller.CurrentHand != BallHand.Left)
                errors.Add("Same-hand hesitation follow-up changed ownership.");

            CompleteCurrentDribble(controller, errors);
        }

        private static void CompleteCurrentDribble(PoundDribbleController controller, ICollection<string> errors)
        {
            for (int i = 0; i < 1000 && controller.IsDribbling; i++) controller.Tick(0.005f);
            if (controller.IsDribbling) errors.Add("Timed dribble failed to complete.");
        }

        private static void AddChunk2(Transform prototype, Transform offense, Transform ball, IReadOnlyDictionary<string, Material> materials)
        {
            Transform leftAnchor = FindOrCreate("LeftHandBallAnchor", offense);
            leftAnchor.localPosition = new Vector3(-0.78f, 1.35f, 0.08f);
            Transform rightAnchor = FindOrCreate("RightHandBallAnchor", offense);
            rightAnchor.localPosition = new Vector3(0.78f, 1.35f, 0.08f);

            EnsureAnchorMarker(leftAnchor, "LeftHandAnchorMarker", materials["leftHand"]);
            EnsureAnchorMarker(rightAnchor, "RightHandAnchorMarker", materials["rightHand"]);

            Transform contact = prototype.Find("BallFloorContactIndicator");
            if (!contact)
            {
                GameObject indicator = Primitive(PrimitiveType.Cylinder, "BallFloorContactIndicator", new Vector3(0, .16f, 0), new Vector3(.55f, .018f, .55f), materials["contact"], prototype);
                contact = indicator.transform;
            }
            else
            {
                Renderer renderer = contact.GetComponent<Renderer>();
                if (renderer) renderer.sharedMaterial = materials["contact"];
            }

            Collider contactCollider = contact.GetComponent<Collider>();
            if (contactCollider) UnityEngine.Object.DestroyImmediate(contactCollider);
            contact.gameObject.SetActive(false);

            PoundDribbleController controller = ball.GetComponent<PoundDribbleController>();
            if (!controller) controller = ball.gameObject.AddComponent<PoundDribbleController>();
            controller.Configure(leftAnchor, rightAnchor, contact.gameObject);
            controller.SetStartingHand(BallHand.Left);
            EditorUtility.SetDirty(controller);
        }

        private static Transform FindOrCreate(string name, Transform parent)
        {
            Transform existing = parent.Find(name);
            return existing ? existing : Group(name, parent);
        }

        private static void EnsureAnchorMarker(Transform anchor, string name, Material material)
        {
            Transform marker = anchor.Find(name);
            if (!marker)
                marker = Primitive(PrimitiveType.Sphere, name, Vector3.zero, Vector3.one * .14f, material, anchor).transform;
            else
            {
                Renderer renderer = marker.GetComponent<Renderer>();
                if (renderer) renderer.sharedMaterial = material;
            }

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider) UnityEngine.Object.DestroyImmediate(markerCollider);
        }

        private static void ValidateHandCycle(PoundDribbleController controller, BallHand hand, Transform expectedAnchor, bool activeIsLeft, GameObject contact, ICollection<string> errors)
        {
            controller.ResetToStartingHand();
            controller.SetStartingHand(hand);
            if (controller.CurrentHand != hand)
                errors.Add(hand + " starting hand did not become the current hand.");
            if (Vector3.Distance(controller.transform.position, expectedAnchor.position) > 0.001f)
                errors.Add(hand + " starting hand did not place the ball at its anchor.");

            bool inactiveAccepted = activeIsLeft ? controller.ProcessInputAtRhythmTime(false, true, 0) : controller.ProcessInputAtRhythmTime(true, false, 0);
            if (inactiveAccepted || controller.IsDribbling)
                errors.Add(hand + " ownership accepted input from the inactive hand.");

            bool started = activeIsLeft ? controller.ProcessInputAtRhythmTime(true, false, 0) : controller.ProcessInputAtRhythmTime(false, true, 0);
            if (!started || !controller.IsDribbling)
                errors.Add(hand + " active input did not start a dribble.");
            int countBefore = controller.CompletedDribbleCount;
            bool sawFloorContact = false;
            bool sawVisibleContact = false;
            for (int i = 0; i < 400 && controller.IsDribbling; i++)
            {
                controller.Tick(0.01f);
                if (controller.LogicalPhase == BallLogicalPhase.FloorContact)
                {
                    sawFloorContact = true;
                    sawVisibleContact |= contact.activeSelf;
                }
            }

            if (controller.IsDribbling) errors.Add(hand + " dribble did not complete in the expected duration.");
            if (!sawFloorContact) errors.Add(hand + " dribble never entered the floor-contact phase.");
            if (!sawVisibleContact) errors.Add(hand + " floor-contact indicator was not visible at contact.");
            if (controller.CompletedDribbleCount != countBefore + 1) errors.Add(hand + " input did not produce exactly one completed dribble.");
            if (controller.CurrentHand != hand) errors.Add(hand + " dribble changed ball ownership.");
            if (Vector3.Distance(controller.transform.position, expectedAnchor.position) > 0.001f) errors.Add(hand + " dribble did not return to the same-hand anchor.");
            if (contact.activeSelf) errors.Add(hand + " floor-contact indicator remained visible after return.");
        }

        private static void AssertUnchanged(string label, Vector3 before, Vector3 after)
        {
            if (Vector3.Distance(before, after) > 0.001f)
                throw new InvalidOperationException(label + " changed during the Chunk 2 upgrade.");
        }

        private static void AssertStill(string label, Vector3 before, Vector3 after, ICollection<string> errors)
        {
            if (Vector3.Distance(before, after) > 0.001f)
                errors.Add(label + " moved during dribble validation.");
        }

        private static void Defender(Transform parent, IReadOnlyDictionary<string, Material> m)
        {
            Transform root = Group("Defender", parent);
            root.position = new Vector3(.35f, 0, 8.15f);
            Primitive(PrimitiveType.Cylinder, "DefenderMarker", new Vector3(0, .125f, 0), new Vector3(1.15f, .025f, 1.15f), m["maroon"], root);
            Primitive(PrimitiveType.Capsule, "Torso", new Vector3(0, 1.35f, 0), new Vector3(.88f, .82f, .68f), m["red"], root, new Vector3(8, 0, 0));
            Primitive(PrimitiveType.Sphere, "Head", new Vector3(0, 2.55f, .06f), Vector3.one * .7f, m["skin"], root);
            Primitive(PrimitiveType.Capsule, "LeftLeg", new Vector3(-.48f, .58f, .08f), new Vector3(.29f, .62f, .29f), m["maroon"], root, new Vector3(0, 0, 20));
            Primitive(PrimitiveType.Capsule, "RightLeg", new Vector3(.48f, .58f, .08f), new Vector3(.29f, .62f, .29f), m["maroon"], root, new Vector3(0, 0, -20));
            Primitive(PrimitiveType.Capsule, "LeftArm", new Vector3(-.84f, 1.65f, .04f), new Vector3(.22f, .78f, .22f), m["red"], root, new Vector3(0, 0, 68));
            Primitive(PrimitiveType.Capsule, "RightArm", new Vector3(.84f, 1.65f, .04f), new Vector3(.22f, .78f, .22f), m["red"], root, new Vector3(0, 0, -68));
        }

        private static void Basket(Transform parent, IReadOnlyDictionary<string, Material> m)
        {
            Transform root = Group("Basket", parent);
            root.position = new Vector3(0, 0, 17.55f);
            Primitive(PrimitiveType.Cylinder, "Pole", new Vector3(0, 2, 1.2f), new Vector3(.34f, 2, .34f), m["metal"], root);
            Primitive(PrimitiveType.Cube, "PoleBase", new Vector3(0, .33f, 1.2f), new Vector3(1.35f, .55f, 1.35f), m["metal"], root);
            Primitive(PrimitiveType.Cube, "SupportArm", new Vector3(0, 3.92f, .62f), new Vector3(.22f, .22f, 1.35f), m["metal"], root);
            Primitive(PrimitiveType.Cube, "Backboard", new Vector3(0, 3.72f, .05f), new Vector3(3.8f, 2.15f, .16f), m["board"], root);
            Transform target = Group("BackboardTarget", root);
            Primitive(PrimitiveType.Cube, "TargetTop", new Vector3(0, 4.1f, -.045f), new Vector3(1.25f, .055f, .03f), m["rim"], target);
            Primitive(PrimitiveType.Cube, "TargetBottom", new Vector3(0, 3.3f, -.045f), new Vector3(1.25f, .055f, .03f), m["rim"], target);
            Primitive(PrimitiveType.Cube, "TargetLeft", new Vector3(-.6f, 3.7f, -.045f), new Vector3(.055f, .85f, .03f), m["rim"], target);
            Primitive(PrimitiveType.Cube, "TargetRight", new Vector3(.6f, 3.7f, -.045f), new Vector3(.055f, .85f, .03f), m["rim"], target);
            Vector3 center = new Vector3(0, 3.05f, -.72f);
            Ring("Rim", center, .48f, 16, .09f, m["rim"], root);
            Primitive(PrimitiveType.Cube, "RimMount", new Vector3(0, 3.05f, -.32f), new Vector3(.16f, .12f, .8f), m["rim"], root);
            Transform net = Group("Net", root);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * 2f / 10f;
                Vector3 top = center + new Vector3(Mathf.Cos(a) * .43f, -.04f, Mathf.Sin(a) * .43f);
                Vector3 bottom = center + new Vector3(Mathf.Cos(a + .32f) * .24f, -.72f, Mathf.Sin(a + .32f) * .24f);
                CylinderBetween("NetStrand_" + (i + 1).ToString("00"), top, bottom, .018f, m["net"], net);
            }
            Ring("NetBottom", center + Vector3.down * .72f, .24f, 10, .025f, m["net"], net);
        }

        private static void Lights(Transform parent)
        {
            Transform group = Group("Lighting", parent);
            GameObject sun = new GameObject("Key Light");
            sun.transform.SetParent(group, false);
            sun.transform.rotation = Quaternion.Euler(48, -32, 0);
            Light key = sun.AddComponent<Light>();
            key.type = LightType.Directional; key.color = new Color(1f, .84f, .68f); key.intensity = 1.35f; key.shadows = LightShadows.Soft;
            PointLight("Front Fill Light", new Vector3(0, 6, -3), new Color(1f, .96f, .9f), 320, 32, group);
            PointLight("Court Fill Light", new Vector3(-3.5f, 7, 5), new Color(.42f, .66f, 1f), 180, 22, group);
            PointLight("Basket Accent Light", new Vector3(4, 6, 14), new Color(1f, .28f, .11f), 160, 12, group);
        }

        private static void PointLight(string name, Vector3 position, Color color, float intensity, float range, Transform parent)
        {
            GameObject go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = position;
            Light light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range; light.shadows = LightShadows.None;
        }

        private static void Camera(Transform parent)
        {
            GameObject go = new GameObject("Main Camera"); go.tag = "MainCamera"; go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0, 4.55f, -7.6f);
            go.transform.rotation = Quaternion.LookRotation(new Vector3(0, 1.35f, 8.5f) - go.transform.position, Vector3.up);
            UnityEngine.Camera cam = go.AddComponent<UnityEngine.Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.018f, .035f, .065f); cam.fieldOfView = 51; cam.nearClipPlane = .2f; cam.farClipPlane = 80; cam.allowHDR = true;
            go.AddComponent<AudioListener>();
        }

        private static Transform Group(string name, Transform parent)
        {
            GameObject go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform;
        }

        private static GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, Transform parent, Vector3? euler = null)
        {
            GameObject go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero); go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }

        private static void Line(string name, Vector3 start, Vector3 end, float thickness, Material material, Transform parent)
        {
            Vector3 direction = end - start;
            GameObject line = Primitive(PrimitiveType.Cube, name, (start + end) * .5f, new Vector3(thickness, thickness * .38f, direction.magnitude), material, parent);
            line.transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private static void Arc(string name, Vector3 center, float radius, float start, float end, int segments, float thickness, Material material, Transform parent, bool towardCamera)
        {
            Transform group = Group(name, parent); float step = (end - start) / segments;
            for (int i = 0; i < segments; i++)
            {
                float a = (start + step * i) * Mathf.Deg2Rad, b = (start + step * (i + 1)) * Mathf.Deg2Rad;
                Vector3 p0 = towardCamera ? center + new Vector3(Mathf.Sin(a) * radius, 0, -Mathf.Cos(a) * radius) : center + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
                Vector3 p1 = towardCamera ? center + new Vector3(Mathf.Sin(b) * radius, 0, -Mathf.Cos(b) * radius) : center + new Vector3(Mathf.Cos(b) * radius, 0, Mathf.Sin(b) * radius);
                Line("Segment_" + (i + 1).ToString("00"), p0, p1, thickness, material, group);
            }
        }

        private static void Ring(string name, Vector3 center, float radius, int segments, float thickness, Material material, Transform parent)
        {
            Transform group = Group(name, parent);
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments, b = (i + 1) * Mathf.PI * 2f / segments;
                Line("Segment_" + (i + 1).ToString("00"), center + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius), center + new Vector3(Mathf.Cos(b) * radius, 0, Mathf.Sin(b) * radius), thickness, material, group);
            }
        }

        private static void CylinderBetween(string name, Vector3 start, Vector3 end, float radius, Material material, Transform parent)
        {
            Vector3 direction = end - start;
            GameObject go = Primitive(PrimitiveType.Cylinder, name, (start + end) * .5f, new Vector3(radius, direction.magnitude * .5f, radius), material, parent);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
        }

        private static GameObject Required(string name, ICollection<string> errors)
        {
            GameObject go = GameObject.Find(name); if (!go) errors.Add("Missing required GameObject: " + name); return go;
        }

        private static void Visible(UnityEngine.Camera camera, GameObject go, string label, ICollection<string> errors)
        {
            if (!go) return;
            Vector3 v = camera.WorldToViewportPoint(go.transform.position + Vector3.up);
            if (v.z <= 0 || v.x < .03f || v.x > .97f || v.y < .03f || v.y > .97f) errors.Add(label + " is outside the comfortable camera viewport: " + v);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child; if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
