// Den här filen är borttagen i samband med refaktoreringen till tools-baserad agent.
// Logiken finns nu i:
//   - Services/ReportTools.cs       (alla rapport-tools)
//   - Services/TestLlmReportAgent.cs (mock-router som anropar tools)
//   - Services/OpenAIReportAgent.cs  (riktig LLM-koppling med samma tools)
//
// Du kan radera den här filen helt – inget refererar till MockReportAgent längre.

namespace BookingDemo.Services;

// Avsiktligt tom.
