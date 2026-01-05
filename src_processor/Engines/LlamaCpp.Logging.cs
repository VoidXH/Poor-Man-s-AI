using System.Diagnostics;

using VoidX.WPF;

namespace PoorMansAI.Engines;

partial class LlamaCpp {
    /// <summary>
    /// Selectively print llama.cpp logs based on the current log level.
    /// </summary>
    void SanitizeLog(object _, DataReceivedEventArgs e) {
        if (e.Data == null) {
            return;
        }

        string line = e.Data;
        if (Logger.MinLogLevel > LogLevel.Debug) {
            for (int i = 0; i < skippedLineStarts.Length; i++) {
                if (line.StartsWith(skippedLineStarts[i])) {
                    return;
                }
            }
            for (int i = 0; i < skippedLineEnds.Length; i++) {
                if (line.EndsWith(skippedLineEnds[i])) {
                    return;
                }
            }

            int index = line.IndexOf('{');
            if (index != -1 && index + 1 != line.Length && (line[index + 1] == '{' || line[index + 1] == '%' || line[index + 1] == '#')) {
                return;
            }
            index = line.LastIndexOf('}');
            if (index > 0 && (line[index - 1] == '%' || line[index - 1] == '}')) {
                return;
            }

            if (line.Length > 18 && line[11..19] == " time = ") {
                string type = line[..11].TrimStart();
                string time = line[19..32].TrimStart();
                string tokens = line[35..40].TrimStart();
                string tps = line[72..80].TrimStart();
                if (type == "eval") {
                    return;
                }
                line = $"{type}: {time}, {tokens} tok. ({tps} tps)";
            }
        }

        if (!string.IsNullOrWhiteSpace(line)) {
            Logger.Log("llama.cpp", line, ConsoleColor.DarkCyan, false);
        }
    }

    /// <summary>
    /// Lines starting with these are only logged when <see cref="Logger.MinLogLevel"/> is lower or equal than <see cref="LogLevel.Debug"/>.
    /// </summary>
    static readonly string[] skippedLineStarts = [
        ".....", ", example_format:", "<start_of_turn>", "<|im_start|>",
        "build:",
        "common_init_from_params:",
        "ggml_metal_device_init:", "ggml_metal_free:", "ggml_metal_init:", "ggml_metal_library_init:",
        "llama_context:", "llama_kv_cache:", "llama_kv_cache_iswa:", "llama_model_loader:", "load_backend:", "load_tensors:",
        "print_info:",
        "slot ", "srv  ", "system info:", "system_info:"
    ];

    /// <summary>
    /// Lines ending with these are only logged when <see cref="Logger.MinLogLevel"/> is lower or equal than <see cref="LogLevel.Debug"/>.
    /// </summary>
    static readonly string[] skippedLineEnds = [
        "<end_of_turn>", "<|im_end|>"
    ];
}
