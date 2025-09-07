using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicSleep
{
    // 실제 사용 예제
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("=== 이전 작업 소요시간 기반 동적 슬립 테스트 ===\n");

            // 테스트 데이터
            var testItems = Enumerable.Range(1, 30).ToList();

            // 1. 가변 처리시간 작업 테스트
            await TestVariableTimeWork(testItems);

            Console.WriteLine("\n" + new string('=', 80) + "\n");
        }

        private static async Task TestVariableTimeWork(IEnumerable<int> items)
        {
            var processor = new DynamicSleepProcessor();

            var result = await processor.ProcessWithDynamicSleepAsync(
                items,
                WorkSimulators.VariableTimeWorkAsync,
                "가변 처리시간 작업");

            processor.PrintDetailedHistory();
        }
    }
}