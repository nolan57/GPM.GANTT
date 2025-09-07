using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GPM.Gantt.ViewModels;
using GPM.Gantt.Models;
using GPM.Gantt.Services;

namespace GPM.Gantt.Tests
{
    [TestClass]
    public class GanttChartViewModelAsyncTests
    {
        private class FakeGanttService : IGanttService
        {
            public List<GanttTask> Seed { get; } = new();
            public Guid LastProjectId { get; private set; }
            public bool DelayBeforeReturn { get; set; }
            public bool DelayDuringCreate { get; set; }
            public bool ThrowOnGet { get; set; }
            public bool ThrowOnCreate { get; set; }
            public int DelayMs { get; set; } = 50;

            public Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken)
            {
                LastProjectId = projectId;
                if (ThrowOnGet)
                {
                    throw new InvalidOperationException("Get failed");
                }
                if (DelayBeforeReturn)
                {
                    return Task.Run(async () =>
                    {
                        await Task.Delay(DelayMs, cancellationToken);
                        return (IEnumerable<GanttTask>)new List<GanttTask>(Seed);
                    }, cancellationToken);
                }
                return Task.FromResult((IEnumerable<GanttTask>)new List<GanttTask>(Seed));
            }

            public async Task<GanttTask> CreateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken)
            {
                LastProjectId = projectId;
                if (ThrowOnCreate)
                {
                    throw new InvalidOperationException("Create failed");
                }
                if (DelayDuringCreate)
                {
                    await Task.Delay(DelayMs, cancellationToken);
                }
                Seed.Add(task);
                return task;
            }

            // Legacy overloads (not used by tests) - delegate to default projectId
            public Task<IEnumerable<GanttTask>> GetTasksAsync(Guid projectId)
                => GetTasksAsync(projectId, CancellationToken.None);
            
            public Task<IEnumerable<GanttTask>> GetTasksAsync(CancellationToken cancellationToken = default)
                => GetTasksAsync(Guid.Empty, cancellationToken);
            public Task<IEnumerable<GanttTask>> GetTasksAsync()
                => GetTasksAsync(Guid.Empty, CancellationToken.None);
            public Task<GanttTask> CreateTaskAsync(GanttTask task, CancellationToken cancellationToken = default)
                => CreateTaskAsync(Guid.Empty, task, cancellationToken);
            public Task<GanttTask> CreateTaskAsync(GanttTask task)
                => CreateTaskAsync(Guid.Empty, task, CancellationToken.None);

            // Other interface members not needed for these tests
            public Task<GanttTask> UpdateTaskAsync(Guid projectId, GanttTask task, CancellationToken cancellationToken)
                => Task.FromResult(task);
            public Task<GanttTask> UpdateTaskAsync(GanttTask task, CancellationToken cancellationToken)
                => Task.FromResult(task);
            public Task<GanttTask> UpdateTaskAsync(GanttTask task)
                => Task.FromResult(task);
            public Task<bool> DeleteTaskAsync(Guid projectId, Guid taskId, CancellationToken cancellationToken)
                => Task.FromResult(true);
            public Task<bool> DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken)
                => Task.FromResult(true);
            public Task<bool> DeleteTaskAsync(Guid taskId)
                => Task.FromResult(true);
            public Task<ValidationResult> ValidateTasksAsync(Guid projectId, IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
                => Task.FromResult(new ValidationResult());
            public Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
                => Task.FromResult(new ValidationResult());
            public Task<ValidationResult> ValidateTasksAsync(IEnumerable<GanttTask> tasks)
                => Task.FromResult(new ValidationResult());
            public Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(Guid projectId, IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
                => Task.FromResult(tasks);
            public Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks, CancellationToken cancellationToken)
                => Task.FromResult(tasks);
            public Task<IEnumerable<GanttTask>> OptimizeScheduleAsync(IEnumerable<GanttTask> tasks)
                => Task.FromResult(tasks);
        }

        [TestMethod]
        public async Task LoadTasksAsync_PopulatesCollections_NoAppDispatcher()
        {
            var fake = new FakeGanttService();
            var t1 = new GanttTask { Title = "A", Start = DateTime.Today, End = DateTime.Today.AddDays(1), RowIndex = 1 };
            var t2 = new GanttTask { Title = "B", Start = DateTime.Today, End = DateTime.Today.AddDays(2), RowIndex = 2 };
            fake.Seed.AddRange(new[] { t1, t2 });
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid() };

            await vm.LoadTasksAsync(CancellationToken.None);

            Assert.AreEqual(2, vm.Tasks.Count);
            Assert.AreEqual(2, vm.TaskModels.Count);
            CollectionAssert.AreEquivalent(new[] { t1, t2 }, new System.Collections.ArrayList(vm.TaskModels));
            Assert.AreEqual(vm.ProjectId, fake.LastProjectId);
            Assert.AreEqual(string.Empty, vm.ErrorMessage);
        }

        [TestMethod]
        public async Task LoadTasksAsync_Canceled_ThrowsAndDoesNotChange()
        {
            var fake = new FakeGanttService { DelayBeforeReturn = true, DelayMs = 100 };
            fake.Seed.Add(new GanttTask { Title = "X", Start = DateTime.Today, End = DateTime.Today.AddDays(1), RowIndex = 1 });
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid() };

            using var cts = new CancellationTokenSource(1);
            await Task.Delay(5);

            try
            {
                await vm.LoadTasksAsync(cts.Token);
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                Assert.AreEqual(0, vm.Tasks.Count);
                Assert.AreEqual(0, vm.TaskModels.Count);
            }
        }

        [TestMethod]
        public async Task LoadTasksAsync_ServiceThrows_PropagatesAndDoesNotChange()
        {
            var fake = new FakeGanttService { ThrowOnGet = true };
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid() };

            try
            {
                await vm.LoadTasksAsync(CancellationToken.None);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(0, vm.Tasks.Count);
                Assert.AreEqual(0, vm.TaskModels.Count);
            }
        }

        [TestMethod]
        public async Task AddTaskAsync_WithProvidedModel_AddsAndSelects()
        {
            var fake = new FakeGanttService();
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid(), StartTime = DateTime.Today };
            var model = new GanttTask { Title = "C", Start = vm.StartTime, End = vm.StartTime.AddDays(3), RowIndex = 1 };

            var addedVm = await vm.AddTaskAsync(model, CancellationToken.None);

            Assert.AreEqual(1, vm.Tasks.Count);
            Assert.AreSame(addedVm, vm.SelectedTask);
            Assert.AreEqual(1, vm.TaskModels.Count);
            Assert.AreSame(model, vm.TaskModels[0]);
            Assert.AreEqual(vm.ProjectId, fake.LastProjectId);
        }

        [TestMethod]
        public async Task AddTaskAsync_DefaultModel_UsesStartTimeAndSelects()
        {
            var fake = new FakeGanttService();
            var vm = new GanttChartViewModel(new ValidationService(), fake)
            {
                ProjectId = Guid.NewGuid(),
                StartTime = new DateTime(2025, 1, 10)
            };

            var addedVm = await vm.AddTaskAsync(null, CancellationToken.None);

            Assert.IsNotNull(addedVm);
            Assert.AreEqual(1, vm.Tasks.Count);
            Assert.AreSame(addedVm, vm.SelectedTask);
            Assert.AreEqual(1, vm.TaskModels.Count);
            var model = vm.TaskModels[0];
            Assert.AreEqual(vm.StartTime, model.Start);
            Assert.AreEqual(vm.StartTime.AddDays(1), model.End);
            Assert.AreEqual(1, model.RowIndex);
        }

        [TestMethod]
        public async Task AddTaskAsync_Canceled_ThrowsAndDoesNotChange()
        {
            var fake = new FakeGanttService { DelayDuringCreate = true, DelayMs = 100 };
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid(), StartTime = DateTime.Today };

            using var cts = new CancellationTokenSource(1);
            await Task.Delay(5);

            try
            {
                await vm.AddTaskAsync(null, cts.Token);
                Assert.Fail("Expected OperationCanceledException");
            }
            catch (OperationCanceledException)
            {
                Assert.AreEqual(0, vm.Tasks.Count);
                Assert.AreEqual(0, vm.TaskModels.Count);
            }
        }

        [TestMethod]
        public async Task AddTaskAsync_ServiceThrows_PropagatesAndDoesNotChange()
        {
            var fake = new FakeGanttService { ThrowOnCreate = true };
            var vm = new GanttChartViewModel(new ValidationService(), fake) { ProjectId = Guid.NewGuid(), StartTime = DateTime.Today };

            try
            {
                await vm.AddTaskAsync(null, CancellationToken.None);
                Assert.Fail("Expected InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
                Assert.AreEqual(0, vm.Tasks.Count);
                Assert.AreEqual(0, vm.TaskModels.Count);
            }
        }
    }
}