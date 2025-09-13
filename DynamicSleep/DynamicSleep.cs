using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicSleep
{
    // 동적 슬립 처리기
    public class DynamicSleepProcessor
    {
        private readonly List<TaskTiming> _taskHistory = new List<TaskTiming>();

        public async Task<ProcessResult> ProcessWithDynamicSleepAsync<T>(
            IEnumerable<T> items,
            Func<T, Task<string>> processor,
            string processName = "Dynamic Sleep Process")
        {
            var result = new ProcessResult { ProcessName = processName };
            var itemList = items.ToList();
            var totalStopwatch = Stopwatch.StartNew();

            Console.WriteLine($"=== {processName} 시작 ===");
            Console.WriteLine($"총 {itemList.Count}개 항목 처리");
            Console.WriteLine("규칙: 이전 작업 <300ms → 50ms 슬립, >300ms → 100ms 슬립\n");

            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                var taskStopwatch = Stopwatch.StartNew();

                try
                {
                    // 실제 작업 처리
                    var taskResult = await processor(item);
                    taskStopwatch.Stop();

                    // 작업 결과 기록
                    var timing = new TaskTiming
                    {
                        Index = i,
                        Item = item?.ToString(),
                        Duration = taskStopwatch.Elapsed,
                        Result = taskResult,
                        ProcessedAt = DateTime.UtcNow
                    };

                    _taskHistory.Add(timing);
                    result.SuccessfulTasks++;
                    result.Results.Add(taskResult);

                    // 동적 슬립 계산 및 적용
                    var sleepTime = CalculateDynamicSleep(i);
                    
                    Console.WriteLine($"[{i + 1:D2}] 작업시간: {timing.Duration.TotalMilliseconds:F0}ms, " +
                                      $"슬립: {sleepTime}ms, 항목: {item}");

                    if (sleepTime > 0 && i < itemList.Count - 1) // 마지막 항목이 아닌 경우만 슬립
                    {
                        await Task.Delay(sleepTime);
                        timing.SleepTime = sleepTime;
                    }
                }
                catch (Exception ex)
                {
                    taskStopwatch.Stop();
                    
                    Console.WriteLine($"[{i + 1:D2}] 작업 실패: {ex.Message}");
                    result.FailedTasks++;
                    result.Errors.Add($"항목 {item}: {ex.Message}");
                }
            }

            totalStopwatch.Stop();
            result.TotalDuration = totalStopwatch.Elapsed;

            PrintSummary(result);
            return result;
        }

        private int CalculateDynamicSleep(int currentIndex)
        {
            // 첫 번째 작업인 경우 기본값
            if (currentIndex == 0)
            {
                return 50; // 기본 슬립
            }

            // 이전 작업의 소요시간 확인
            var previousTask = _taskHistory[currentIndex - 1];
            var previousDurationMs = previousTask.Duration.TotalMilliseconds;

            // 동적 슬립 규칙 적용
            if (previousDurationMs < 300)
            {
                return 50;  // 빠른 작업 → 짧은 슬립
            }
            else
            {
                return 100; // 느린 작업 → 긴 슬립
            }
        }

        private void PrintSummary(ProcessResult result)
        {
            Console.WriteLine($"\n=== {result.ProcessName} 완료 ===");
            Console.WriteLine($"총 소요시간: {result.TotalDuration.TotalSeconds:F1}초");
            Console.WriteLine($"성공: {result.SuccessfulTasks}, 실패: {result.FailedTasks}");
            
            if (_taskHistory.Any())
            {
                var avgTaskTime = _taskHistory.Average(t => t.Duration.TotalMilliseconds);
                var avgSleepTime = _taskHistory.Where(t => t.SleepTime > 0).Average(t => t.SleepTime);
                
                Console.WriteLine($"평균 작업시간: {avgTaskTime:F0}ms");
                Console.WriteLine($"평균 슬립시간: {avgSleepTime:F0}ms");
                
                // 슬립 패턴 분석
                var shortSleeps = _taskHistory.Count(t => t.SleepTime == 50);
                var longSleeps = _taskHistory.Count(t => t.SleepTime == 100);
                
                Console.WriteLine($"50ms 슬립: {shortSleeps}회, 100ms 슬립: {longSleeps}회");
            }
        }

        public void PrintDetailedHistory()
        {
            Console.WriteLine("\n=== 상세 실행 히스토리 ===");
            Console.WriteLine("Index | 작업시간 | 슬립시간 | 누적시간 | 항목");
            Console.WriteLine(new string('-', 55));

            var cumulativeTime = 0.0;
            foreach (var task in _taskHistory)
            {
                cumulativeTime += task.Duration.TotalMilliseconds + task.SleepTime;
                
                Console.WriteLine($"{task.Index + 1,5} | " +
                                  $"{task.Duration.TotalMilliseconds,6:F0}ms | " +
                                  $"{task.SleepTime,6}ms | " +
                                  $"{cumulativeTime,6:F0}ms | " +
                                  $"{task.Item}");
            }
        }
    }

    // 작업 시뮬레이터
    public class WorkSimulators
    {
        private static readonly Random _random = new Random();

        // 가변 처리시간 작업
        public static async Task<string> VariableTimeWorkAsync(int itemId)
        {
            var processingTime = _random.Next(50, 500);
            await Task.Delay(processingTime);
            
            return $"Variable_{itemId}_{processingTime}ms";
        }
    }

    // 데이터 모델들
    public class TaskTiming
    {
        public int Index { get; set; }
        public string Item { get; set; }
        public TimeSpan Duration { get; set; }
        public string Result { get; set; }
        public DateTime ProcessedAt { get; set; }
        public int SleepTime { get; set; }
    }

    public class ProcessResult
    {
        public string ProcessName { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public int SuccessfulTasks { get; set; }
        public int FailedTasks { get; set; }
        public List<string> Results { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}