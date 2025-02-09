using System.Text.Json;

namespace PoorMansAI.VoidMoA {
    /// <summary>
    /// Prompt created by <see cref="PromptTransformer"/>, extracted positive and negative prompts and a selected model.
    /// </summary>
    public class TransformedPrompt {
        /// <summary>
        /// Retained prompts to be sent to the generator.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Negative prompts, things not to generate.
        /// </summary>
        public string NegativePrompt { get; set; }

        /// <summary>
        /// Name of the used model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Image width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Image height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Convert the prompt to a Stable Diffusion JSON API call.
        /// </summary>
        public override string ToString() => JsonSerializer.Serialize(new {
            prompt = Prompt,
            negative_prompt = NegativePrompt,
            width = Width,
            height = Height,
            steps = 20,
            override_settings = new {
                sd_model_checkpoint = Model
            }
        });
    }
}