using System.Text.Json;

using PoorMansAI.Configuration;

namespace PoorMansAI.NewTech.VoidMoA {
    /// <summary>
    /// Prompt created by <see cref="PromptTransformer"/>, extracted positive and negative prompts and a selected model.
    /// </summary>
    public class TransformedPrompt {
        /// <summary>
        /// Type of process to use (txt2img, img2img,...).
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Retained prompts to be sent to the generator.
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// Negative prompts, things not to generate.
        /// </summary>
        public string NegativePrompt { get; set; }

        /// <summary>
        /// Images to transform with the prompt.
        /// </summary>
        public string[] ReferenceImages { get; set; }

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
        /// CFG scale, how strongly to follow the prompt.
        /// </summary>
        public float Guidance { get; set; }

        /// <summary>
        /// Which sampling method to use.
        /// </summary>
        public string Sampler { get; set; }

        /// <summary>
        /// Convert the prompt to a Stable Diffusion JSON API call.
        /// </summary>
        public override string ToString() => JsonSerializer.Serialize(new {
            prompt = Prompt,
            negative_prompt = NegativePrompt,
            init_images = ReferenceImages,
            width = Width,
            height = Height,
            steps = Config.imageGenSteps,
            cfg_scale = Guidance,
            sampler_index = Sampler,
            override_settings = new {
                sd_model_checkpoint = Model
            }
        });
    }
}