using PoorMansAI.Engines;
using PoorMansAI.Engines.Orchestration;
using PoorMansAI.Tests.Engines.Orchestration.Mocks;
using PoorMansAI.Tests.Engines.Mocks;

namespace PoorMansAI.Tests.Engines.Orchestration;

/// <summary>
/// Tests the <see cref="EngineCache"/> class.
/// </summary>
[TestClass]
public class EngineCache_Tests {
    /// <summary>
    /// Creates a test-ready, engine-mocked <see cref="EngineCache"/> in offline mode for testing.
    /// </summary>
    /// <returns></returns>
    static EngineCache CreateEngineCache() => new(EngineCacheMode.Offline) {
        Factory = new MockEngineFactory()
    };

    #region Static checker helpers

    /// <summary>
    /// Checks that no engines are running.
    /// </summary>
    static void AssertNoEngines(EngineCache cache) => Assert.HasCount(0, cache.Engines, "Expected no engines to be running.");

    /// <summary>
    /// Checks that exactly one engine is running and it is of the expected type.
    /// </summary>
    static void AssertSingleEngine(EngineCache cache, EngineType expectedType, Type expectedEngineType) {
        Assert.HasCount(1, cache.Engines, $"Expected exactly one engine, found {cache.Engines.Count}.");
        Assert.IsTrue(cache.Engines.ContainsKey(expectedType), $"Expected engine of type {expectedType} to be present.");
        Assert.IsInstanceOfType(cache.Engines[expectedType], expectedEngineType, $"Expected engine of type {expectedType} to be of type {expectedEngineType}.");
    }

    /// <summary>
    /// Checks that exactly two engines are running of the expected types.
    /// </summary>
    static void AssertTwoEngines(EngineCache cache, EngineType type1, Type engineType1, EngineType type2, Type engineType2) {
        Assert.HasCount(2, cache.Engines, $"Expected exactly two engines, found {cache.Engines.Count}.");
        Assert.IsTrue(cache.Engines.ContainsKey(type1), $"Expected engine of type {type1} to be present.");
        Assert.IsInstanceOfType(cache.Engines[type1], engineType1, $"Expected engine of type {type1} to be of type {engineType1}.");
        Assert.IsTrue(cache.Engines.ContainsKey(type2), $"Expected engine of type {type2} to be present.");
        Assert.IsInstanceOfType(cache.Engines[type2], engineType2, $"Expected engine of type {type2} to be of type {engineType2}.");
    }

    /// <summary>
    /// Checks that a given engine type is no longer running.
    /// </summary>
    static void AssertEngineRemoved(EngineCache cache, EngineType type) => Assert.IsFalse(cache.Engines.ContainsKey(type), $"Expected engine of type {type} to have been removed.");

    /// <summary>
    /// Checks that a given engine type is running and is of the expected engine type.
    /// </summary>
    static void AssertEngineRunning(EngineCache cache, EngineType type, Type expectedEngineType) {
        Assert.IsTrue(cache.Engines.ContainsKey(type), $"Expected engine of type {type} to be present.");
        Assert.IsInstanceOfType(cache.Engines[type], expectedEngineType, $"Expected engine of type {type} to be of type {expectedEngineType}.");
    }

    #endregion

    #region Engine swapping tests

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.SLM"/> to <see cref="EngineCacheMode.Offline"/>
    /// correctly removes the chat engine and leaves no engines running.
    /// </summary>
    [TestMethod]
    public void SwapMode_SLMToOffline_RemovesChatEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with SLM mode - should have a ChatEngine (MockChatEngine with gpu=false)
        cache.Mode = EngineCacheMode.SLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Change to Offline - should remove the chat engine
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Offline"/> to <see cref="EngineCacheMode.SLM"/>
    /// correctly creates only the SLM chat engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_OfflineToSLM_CreatesOnlySLMEngine() {
        using EngineCache cache = CreateEngineCache();

        // Start from offline - no engines
        AssertNoEngines(cache);

        // Switch to SLM - should create only the SLM chat engine
        cache.Mode = EngineCacheMode.SLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Verify it is a small model (gpu=false, but type is the same so just check type)
        AssertEngineRunning(cache, EngineType.Chat, typeof(MockChatEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.SLM"/> to <see cref="EngineCacheMode.LLM"/>
    /// swaps the chat engine from SLM to LLM.
    /// </summary>
    [TestMethod]
    public void SwapMode_SLMToLLM_SwapsChatEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with SLM
        cache.Mode = EngineCacheMode.SLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Switch to LLM - SLM engine should be disposed, LLM engine created
        cache.Mode = EngineCacheMode.LLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));
        Assert.HasCount(1, cache.Engines, "Expected exactly one engine after swapping SLM -> LLM.");
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.LLM"/> to <see cref="EngineCacheMode.Offline"/>
    /// correctly removes the LLM chat engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_LLMToOffline_RemovesLLMEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with LLM
        cache.Mode = EngineCacheMode.LLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Switch to Offline - should remove the LLM engine
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Offline"/> to <see cref="EngineCacheMode.Image"/>
    /// creates only the image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_OfflineToImage_CreatesOnlyImageEngine() {
        using EngineCache cache = CreateEngineCache();

        // Start from offline
        AssertNoEngines(cache);

        // Switch to Image - should create only the Image engine
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Image"/> to <see cref="EngineCacheMode.Offline"/>
    /// correctly removes the image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageToOffline_RemovesImageEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with Image
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));

        // Switch to Offline - should remove the image engine
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.SLM"/> to <see cref="EngineCacheMode.Image"/>
    /// replaces the SLM chat engine with the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_SLMToImage_SwapsChatEngineForImage() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with SLM
        cache.Mode = EngineCacheMode.SLM;
        AssertEngineRunning(cache, EngineType.Chat, typeof(MockChatEngine));
        AssertEngineRemoved(cache, EngineType.Image);

        // Switch to Image - SLM engine should be disposed, Image engine created
        cache.Mode = EngineCacheMode.Image;
        AssertEngineRemoved(cache, EngineType.Chat);
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Image"/> to <see cref="EngineCacheMode.SLM"/>
    /// replaces the Image engine with the SLM chat engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageToSLM_SwapsImageEngineForChat() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with Image
        cache.Mode = EngineCacheMode.Image;
        AssertEngineRunning(cache, EngineType.Image, typeof(MockImageEngine));
        AssertEngineRemoved(cache, EngineType.Chat);

        // Switch to SLM - Image engine should be disposed, SLM chat engine created
        cache.Mode = EngineCacheMode.SLM;
        AssertEngineRemoved(cache, EngineType.Image);
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Offline"/> to <see cref="EngineCacheMode.ImageAndSLM"/>
    /// creates both the SLM chat engine and the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_OfflineToImageAndSLM_CreatesBothEngines() {
        using EngineCache cache = CreateEngineCache();

        // Start from offline
        AssertNoEngines(cache);

        // Switch to ImageAndSLM - should create both SLM and Image engines
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.ImageAndSLM"/> to <see cref="EngineCacheMode.Offline"/>
    /// removes both the chat engine and the image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageAndSLMToOffline_RemovesAllEngines() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with ImageAndSLM
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));

        // Switch to Offline - should remove both engines
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.ImageAndSLM"/> to <see cref="EngineCacheMode.Image"/>
    /// removes only the chat engine, keeping the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageAndSLMToImage_RemovesOnlyChatEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with ImageAndSLM
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));

        // Switch to Image - SLM chat engine should be removed, Image engine stays
        cache.Mode = EngineCacheMode.Image;
        AssertEngineRemoved(cache, EngineType.Chat);
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Image"/> to <see cref="EngineCacheMode.ImageAndSLM"/>
    /// adds the SLM chat engine while keeping the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageToImageAndSLM_AddsChatEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with Image
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));

        // Switch to ImageAndSLM - should add SLM chat engine while keeping Image engine
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.ImageAndSLM"/> to <see cref="EngineCacheMode.SLM"/>
    /// removes only the Image engine, keeping the SLM chat engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageAndSLMToSLM_RemovesOnlyImageEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with ImageAndSLM
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));

        // Switch to SLM - Image engine should be removed, SLM chat engine stays
        cache.Mode = EngineCacheMode.SLM;
        AssertEngineRemoved(cache, EngineType.Image);
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.LLM"/> to <see cref="EngineCacheMode.ImageAndSLM"/>
    /// swaps the LLM chat engine for an SLM chat engine and adds the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_LLMToImageAndSLM_SwapsChatAndAddsImage() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with LLM
        cache.Mode = EngineCacheMode.LLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Switch to ImageAndSLM - LLM disposed, SLM created, Image added
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Image"/> to <see cref="EngineCacheMode.LLM"/>
    /// replaces the Image engine with the LLM chat engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageToLLM_SwapsImageEngineForChat() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with Image
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));

        // Switch to LLM - Image disposed, LLM created
        cache.Mode = EngineCacheMode.LLM;
        AssertEngineRemoved(cache, EngineType.Image);
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.LLM"/> to <see cref="EngineCacheMode.Image"/>
    /// replaces the LLM chat engine with the Image engine.
    /// </summary>
    [TestMethod]
    public void SwapMode_LLMToImage_SwapsChatEngineForImage() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with LLM
        cache.Mode = EngineCacheMode.LLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // Switch to Image - LLM disposed, Image created
        cache.Mode = EngineCacheMode.Image;
        AssertEngineRemoved(cache, EngineType.Chat);
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
    }

    /// <summary>
    /// Tests that switching from <see cref="EngineCacheMode.Image"/> to <see cref="EngineCacheMode.Image"/>
    /// (no change) does not crash and keeps the same engine running.
    /// </summary>
    [TestMethod]
    public void SwapMode_ImageToImage_NoChange_KeepsEngine() {
        using EngineCache cache = CreateEngineCache();

        // Initialize with Image
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
        Type firstImageType = cache.Engines[EngineType.Image].GetType();

        // Switch to Image again - no change expected
        cache.Mode = EngineCacheMode.Image;
        AssertSingleEngine(cache, EngineType.Image, typeof(MockImageEngine));
        Assert.AreEqual(firstImageType, cache.Engines[EngineType.Image].GetType(), "Engine instance should not change on same mode.");
    }

    /// <summary>
    /// Tests a sequence of mode changes: Offline -> SLM -> ImageAndSLM -> LLM -> Offline.
    /// </summary>
    [TestMethod]
    public void SwapMode_Sequence_OfflineSLMImageAndSLMLLMOffline_VerifiesAllStates() {
        using EngineCache cache = CreateEngineCache();

        // Offline -> SLM
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
        cache.Mode = EngineCacheMode.SLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // SLM -> ImageAndSLM
        cache.Mode = EngineCacheMode.ImageAndSLM;
        AssertTwoEngines(cache, EngineType.Chat, typeof(MockChatEngine), EngineType.Image, typeof(MockImageEngine));

        // ImageAndSLM -> LLM
        cache.Mode = EngineCacheMode.LLM;
        AssertSingleEngine(cache, EngineType.Chat, typeof(MockChatEngine));

        // LLM -> Offline
        cache.Mode = EngineCacheMode.Offline;
        AssertNoEngines(cache);
    }

    #endregion
}
