using System.IO;
using System.Linq;
using Dashblade.Flow;
using NUnit.Framework;
using UnityEditor;

namespace Dashblade.Tests.EditMode
{
    /// <summary>
    /// F06 set-up: which scene each choice loads, the build scene list (criterion 10) and the EndingConfig asset
    /// (criterion 12).
    /// </summary>
    public sealed class GameFlowSetupTests
    {
        private const string EndingConfigPath = "Assets/_Game/Config/EndingConfig.asset";
        private const string MainMenuScenePath = "Assets/_Game/Scenes/MainMenu.unity";
        private const string ArenaScenePath = "Assets/_Game/Scenes/Arena.unity";

        [TestCase(MenuChoice.Play, GameScenes.Arena)]
        [TestCase(MenuChoice.Retry, GameScenes.Arena)]
        [TestCase(MenuChoice.Menu, GameScenes.MainMenu)]
        public void Choice_LoadsItsScene(MenuChoice choice, string scene)
        {
            Assert.That(GameScenes.SceneFor(choice), Is.EqualTo(scene));
        }

        [Test]
        public void Quit_LoadsNoScene()
        {
            Assert.That(GameScenes.SceneFor(MenuChoice.Quit), Is.Null);
        }

        [Test]
        public void SceneNames_MatchTheSceneFiles()
        {
            Assert.That(Path.GetFileNameWithoutExtension(MainMenuScenePath), Is.EqualTo(GameScenes.MainMenu));
            Assert.That(Path.GetFileNameWithoutExtension(ArenaScenePath), Is.EqualTo(GameScenes.Arena));
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ArenaScenePath), Is.Not.Null);
        }

        [Test]
        public void BuildSceneList_IsMainMenuThenArena_BothEnabled()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            Assert.That(scenes.Select(s => s.path).ToArray(), Is.EqualTo(new[] { MainMenuScenePath, ArenaScenePath }));
            Assert.That(scenes.All(s => s.enabled), Is.True);
        }

        [Test]
        public void EndingConfig_HasTheSpecValues()
        {
            var config = AssetDatabase.LoadAssetAtPath<EndingConfig>(EndingConfigPath);
            Assert.That(config, Is.Not.Null, $"No {nameof(EndingConfig)} at {EndingConfigPath}.");
            Assert.That(config.EndingDelay, Is.EqualTo(1f));
            Assert.That(config.DimStrength, Is.EqualTo(0.7f));
        }

        [TestCase("endingDelay", "Ending Delay")]
        [TestCase("dimStrength", "Dim Strength")]
        public void EndingConfig_ShowsTheSpecLabels(string field, string label)
        {
            var config = AssetDatabase.LoadAssetAtPath<EndingConfig>(EndingConfigPath);
            Assert.That(config, Is.Not.Null, $"No {nameof(EndingConfig)} at {EndingConfigPath}.");

            using (var serialized = new SerializedObject(config))
            {
                SerializedProperty property = serialized.FindProperty(field);
                Assert.That(property, Is.Not.Null);
                Assert.That(property.displayName, Is.EqualTo(label));
            }
        }
    }
}
