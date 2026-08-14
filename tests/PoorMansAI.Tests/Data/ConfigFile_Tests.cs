using VoidX.WPF;

using PoorMansAI.Configuration;
using PoorMansAI.Engines.Models;
using PoorMansAI.Tests.Utilities;

namespace PoorMansAI.Tests.Data;

/// <summary>
/// Verifies that configuration files and INI imports load correctly.
/// </summary>
[TestClass]
public class ConfigFile_Tests : TestWithContext {
    /// <summary>
    /// Files to test.
    /// </summary>
    static readonly string[] DefaultConfigFileNames = [
        "6 GB GPU.ini",
        "16 GB GPU.ini",
        "32 GB Apple.ini",
        "64 GB Apple.ini"
    ];

    /// <summary>
    /// Verifies that all default configuration files exist, can be loaded and parsed, and correctly resolve imports.
    /// </summary>
    [TestMethod]
    public void DefaultConfigFiles_CanBeLoadedAndParsed() {
        string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration");
        if (!Directory.Exists(configDir)) {
            configDir = Path.Combine(Directory.GetCurrentDirectory(), "Configuration");
        }

        Assert.IsTrue(Directory.Exists(configDir), $"Configuration directory not found at {configDir}");

        foreach (string fileName in DefaultConfigFileNames) {
            string filePath = Path.Combine(configDir, fileName);
            Assert.IsTrue(File.Exists(filePath), $"Default config file not found: {filePath}");

            // Verify IniFile.Parse returns blocks
            IniFileBlock[] blocks = IniFile.Parse(filePath);
            Assert.IsNotNull(blocks);
            Assert.IsNotEmpty(blocks, $"No blocks parsed from {fileName}");

            // Verify IniFile.ParseAll returns key-value pairs including imported ones
            Dictionary<string, string> configData = IniFile.ParseAll(filePath);
            Assert.IsNotNull(configData);
            Assert.IsNotEmpty(configData, $"No values parsed from {fileName}");

            // Verify keys imported from referenced advanced files are present
            Assert.IsTrue(configData.ContainsKey("ChatTemperature"), $"ChatTemperature missing in {fileName}");
            Assert.IsTrue(configData.ContainsKey("ChatPresencePenalty"), $"ChatPresencePenalty missing in {fileName}");
            Assert.IsTrue(configData.ContainsKey("Agent1CopilotModel"), $"Agent1CopilotModel missing in {fileName}");
            Assert.IsTrue(configData.ContainsKey("ImageGenSteps"), $"ImageGenSteps missing in {fileName}");
            Assert.IsTrue(configData.ContainsKey("MoAModel"), $"MoAModel missing in {fileName}");

            // Overwrite active Config with current file's parsed configuration
            Config.Overwrite(configData);

            // Validate Model Names assigned by Config
            string[] modelNames = [.. Config.GetModelNames()];
            Assert.IsNotEmpty(modelNames, $"No model names found in {fileName}");

            // Validate LlamaCppSettings and LLModel properties assigned from Config
            foreach (bool llm in new[] { false, true }) {
                LlamaCppSettings settings = new(llm);
                Assert.IsGreaterThan(0, settings.Port);
                Assert.IsGreaterThan(0, settings.Timeout);
                Assert.IsGreaterThanOrEqualTo(0, settings.Loading);
                Assert.IsGreaterThan(0, settings.Context);

                Dictionary<string, LLModel> models = settings.GetConfiguredModels();
                Assert.IsGreaterThan(0, models.Count, $"No configured models loaded for llm={llm} in {fileName}");

                foreach (LLModel model in models.Values) {
                    Assert.IsFalse(string.IsNullOrEmpty(model.Name));
                    Assert.IsFalse(string.IsNullOrEmpty(model.FilePath));
                    Assert.IsNotNull(model.SystemMessage);
                    Assert.IsGreaterThanOrEqualTo(0.0f, model.Temperature);
                    Assert.IsGreaterThanOrEqualTo(0.0f, model.MinP);
                    Assert.IsGreaterThanOrEqualTo(0.0f, model.PresencePenalty);
                }
            }

            // Validate AgentSettings assigned from Config
            AgentSettings[] agentSettingsList = [.. Config.GetAllAgentSettings()];
            Assert.IsNotEmpty(agentSettingsList, $"No agent settings loaded in {fileName}");
            foreach (AgentSettings agentSetting in agentSettingsList) {
                Assert.IsFalse(string.IsNullOrEmpty(agentSetting.Command));
            }

            Dictionary<string, AgentModel> configuredAgents = AgentSettings.GetConfiguredAgents();
            Assert.IsNotEmpty(configuredAgents, $"No configured agents found in {fileName}");
            foreach (AgentModel agentModel in configuredAgents.Values) {
                Assert.IsFalse(string.IsNullOrEmpty(agentModel.Name));
            }
        }
    }

    /// <summary>
    /// Verifies that Config.Overwrite(filePath) loads each default configuration file directly without crashing.
    /// </summary>
    [TestMethod]
    public void Config_OverwriteWithFilePath_LoadsWithoutCrashing() {
        string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration");
        if (!Directory.Exists(configDir)) {
            configDir = Path.Combine(Directory.GetCurrentDirectory(), "Configuration");
        }

        foreach (string fileName in DefaultConfigFileNames) {
            string filePath = Path.Combine(configDir, fileName);
            Config.Overwrite(filePath);

            string[] modelNames = [.. Config.GetModelNames()];
            Assert.IsNotEmpty(modelNames);

            LlamaCppSettings settings = new(llm: false);
            Dictionary<string, LLModel> models = settings.GetConfiguredModels();
            Assert.IsGreaterThan(0, models.Count);
        }
    }

    /// <summary>
    /// Reset the configuration repository for further tests.
    /// </summary>
    [TestCleanup]
    public void TestCleanup() => Config.Overwrite((Dictionary<string, string>)null);
}
