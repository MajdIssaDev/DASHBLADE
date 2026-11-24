using Dashblade.Combat;
using NUnit.Framework;
using UnityEditor;

namespace Dashblade.Tests.EditMode
{
    /// <summary>The real DashConfig asset holds the spec values (F02, and Start Lockout from F06 criterion 14).</summary>
    public sealed class DashConfigAssetTests
    {
        private const string DashConfigPath = "Assets/_Game/Config/DashConfig.asset";

        private static DashConfig Load()
        {
            var config = AssetDatabase.LoadAssetAtPath<DashConfig>(DashConfigPath);
            Assert.That(config, Is.Not.Null, $"No {nameof(DashConfig)} at {DashConfigPath}.");
            return config;
        }

        [Test]
        public void DashConfig_HasTheSpecValues()
        {
            DashConfig config = Load();
            Assert.That(config.Distance, Is.EqualTo(5f));
            Assert.That(config.Duration, Is.EqualTo(0.15f));
            Assert.That(config.MissCooldown, Is.EqualTo(1f));
            Assert.That(config.HitWidth, Is.EqualTo(1.25f));
            Assert.That(config.StartLockout, Is.EqualTo(0.5f));
        }

        [Test]
        public void StartLockout_IsSavedInTheAsset_NotOnlyTheScriptDefault()
        {
            using (var serialized = new SerializedObject(Load()))
            {
                SerializedProperty property = serialized.FindProperty("startLockout");
                Assert.That(property, Is.Not.Null);
                Assert.That(property.displayName, Is.EqualTo("Start Lockout"));
            }

            Assert.That(System.IO.File.ReadAllText(DashConfigPath), Does.Contain("startLockout: 0.5"));
        }
    }
}
