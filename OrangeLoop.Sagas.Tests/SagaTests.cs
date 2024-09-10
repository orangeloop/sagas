using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Tests
{
    [TestClass]
    public class SagaTests
    {
        private ISagaFactory sagaFactory = new SagaFactory();

        [TestMethod]
        public async Task Saga_Run_AllStepsAreExecuted()
        {
            // Arrange
            var saga = sagaFactory
                .Create<SagaContext>()
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step1_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step1_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step2_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step2_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step3_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step3_Rollback = true;
                        await success(context);
                    });
                });

            // Act
            var result = await saga.Run(new SagaContext());

            // Assert
            Assert.IsTrue(result.Value.Step1_Invoked);
            Assert.IsTrue(result.Value.Step2_Invoked);
            Assert.IsTrue(result.Value.Step3_Invoked);

            Assert.IsFalse(result.Value.Step1_Rollback);
            Assert.IsFalse(result.Value.Step2_Rollback);
            Assert.IsFalse(result.Value.Step3_Rollback);
        }

        [TestMethod]
        public async Task Saga_Run_AllStepsAreRolledBack()
        {
            // Arrange
            var saga = sagaFactory
                .Create<SagaContext>()
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step1_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step1_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step2_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step2_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step3_Invoked = true;
                        await failure(context, new Exception());
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step3_Rollback = true;
                        await success(context);
                    });
                });

            // Act
            var result = await saga.Run(new SagaContext());

            // Assert
            Assert.IsTrue(result.Value.Step1_Invoked);
            Assert.IsTrue(result.Value.Step2_Invoked);
            Assert.IsTrue(result.Value.Step3_Invoked);

            Assert.IsTrue(result.Value.Step1_Rollback);
            Assert.IsTrue(result.Value.Step2_Rollback);
            Assert.IsTrue(result.Value.Step3_Rollback);

            Assert.IsInstanceOfType<AggregateException>(result.Error);
            Assert.AreEqual(1, result.Error.InnerExceptions.Count);
        }

        [TestMethod]
        public async Task Saga_Run_AllStepsAreRolledBack_WhenRollbackFails()
        {
            // Arrange
            var saga = sagaFactory
                .Create<SagaContext>()
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step1_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step1_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step2_Invoked = true;
                        await success(context);
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step2_Rollback = true;
                        await success(context);
                    });
                })
                .AddStep(step =>
                {
                    step.OnExecute(async (context, success, failure) =>
                    {
                        context.Step3_Invoked = true;
                        await failure(context, new Exception("OnExecute Error"));
                    });

                    step.OnRollback(async (context, success, failure) =>
                    {
                        context.Step3_Rollback = true;
                        await failure(context, new Exception("OnRollback Error"));
                    });
                });

            // Act
            var result = await saga.Run(new SagaContext());

            // Assert
            Assert.IsTrue(result.Value.Step1_Invoked);
            Assert.IsTrue(result.Value.Step2_Invoked);
            Assert.IsTrue(result.Value.Step3_Invoked);

            Assert.IsTrue(result.Value.Step1_Rollback);
            Assert.IsTrue(result.Value.Step2_Rollback);
            Assert.IsTrue(result.Value.Step3_Rollback);

            Assert.IsInstanceOfType<AggregateException>(result.Error);
            Assert.AreEqual(2, result.Error.InnerExceptions.Count);
            Assert.AreEqual("OnExecute Error", result.Error.InnerExceptions[0].Message);
            Assert.AreEqual("OnRollback Error", result.Error.InnerExceptions[1].Message);
        }

    }

    #region Sample Sagas for Testing
    public class SagaContext
    {
        public bool Step1_Invoked { get; set; } = false;
        public bool Step2_Invoked { get; set; } = false;
        public bool Step3_Invoked { get; set; } = false;
        public bool Step1_Rollback { get; set; } = false;
        public bool Step2_Rollback { get; set; } = false;
        public bool Step3_Rollback { get; set; } = false;
    }
    #endregion
}
