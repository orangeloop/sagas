using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Tests
{
    [TestClass]
    public class SagaTests
    {
        //private AutoMoq.AutoMoqer _mocker = new AutoMoq.AutoMoqer();
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public SagaTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.Commit());
            _unitOfWorkMock.Setup(x => x.Rollback());

            _unitOfWorkFactoryMock = new Mock<IUnitOfWorkFactory>();
            _unitOfWorkFactoryMock.Setup(x => x.Create()).Returns(_unitOfWorkMock.Object);
        }

        [TestMethod]
        public async Task Saga_Run_All_Steps_Are_Executed()
        {
            // Arrange
            var saga = new SimpleSaga();

            // Act
            var context = await saga.Run(new SagaContext()).ConfigureAwait(false);

            // Assert
            Assert.IsTrue(context.Step1_Invoked);
            Assert.IsTrue(context.Step2_Invoked);
            Assert.IsTrue(context.Step3_Invoked);

            Assert.IsFalse(context.Step1_Rollback);
            Assert.IsFalse(context.Step2_Rollback);
            Assert.IsFalse(context.Step3_Rollback);
        }

        [TestMethod]
        public async Task Saga_Run_All_Steps_Are_Rolled_Back()
        {
            // Arrange
            var saga = new SimpleSagaWithRollback();

            // Act
            var context = new SagaContext();
            try
            {
                context = await saga.Run(context).ConfigureAwait(false);

                Assert.IsTrue(false, "Expected Exception");
            }
            catch
            {
                // Assert
                Assert.IsTrue(context.Step1_Invoked);
                Assert.IsTrue(context.Step2_Invoked);
                Assert.IsFalse(context.Step3_Invoked);

                Assert.IsTrue(context.Step1_Rollback);
                Assert.IsTrue(context.Step2_Rollback);
                Assert.IsTrue(context.Step3_Rollback);
            }
        }

        [TestMethod]
        public async Task UnitOfWorkSaga_CallsCommit()
        {
            // Arrange
            var saga = new SimpleUOWSaga(_unitOfWorkFactoryMock.Object);

            // Act
            var context = await saga.Run(new SagaContext());

            // Assert
            Assert.IsTrue(context.Step1_Invoked);
            Assert.IsTrue(context.Step2_Invoked);
            Assert.IsTrue(context.Step3_Invoked);
            _unitOfWorkMock.Verify(x => x.Commit(), Times.Once());
            _unitOfWorkMock.Verify(x => x.Rollback(), Times.Never());
        }

        [TestMethod]
        public async Task UnitOfWorkSaga_CallsRollback()
        {
            // Arrange
            var saga = new SimpleUOWSagaWithRollback(_unitOfWorkFactoryMock.Object);

            // Act
            var context = new SagaContext();
            try
            {
                context = await saga.Run(context).ConfigureAwait(false);

                Assert.IsTrue(false, "Expected Exception");
            }
            catch
            {
                // Assert
                Assert.IsTrue(context.Step1_Invoked);
                Assert.IsTrue(context.Step2_Invoked);
                Assert.IsFalse(context.Step3_Invoked);

                Assert.IsTrue(context.Step1_Rollback);
                Assert.IsTrue(context.Step2_Rollback);
                Assert.IsTrue(context.Step3_Rollback);

                _unitOfWorkMock.Verify(x => x.Commit(), Times.Never());
                _unitOfWorkMock.Verify(x => x.Rollback(), Times.Once());
            }
        }
    }

    #region Sample Sagas for Testing
    public class SagaContext
    {
        public bool Step1_Invoked { get; set; }
        public bool Step2_Invoked { get; set; }
        public bool Step3_Invoked { get; set; }
        public bool Step1_Rollback { get; set; }
        public bool Step2_Rollback { get; set; }
        public bool Step3_Rollback { get; set; }
    }

    public class Step1 : SagaStep<SagaContext>
    {
        public Step1()
        {
            this.ExecuteMethod = context => { context.Step1_Invoked = true; return Task.FromResult(context); };
            this.RollbackMethod = context => { context.Step1_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class Step2 : SagaStep<SagaContext>
    {
        public Step2() : base(context => { context.Step2_Invoked = true; return Task.FromResult(context); }, context => { context.Step2_Rollback = true; return Task.FromResult(context); }) { }
    }

    public class Step3 : SagaStep<SagaContext>
    {
        public Step3() : base(context => { context.Step3_Invoked = true; return Task.FromResult(context); }, context => { context.Step3_Rollback = true; return Task.FromResult(context); }) { }
    }

    public class Step3WithException : SagaStep<SagaContext>
    {
        public Step3WithException()
        {
            this.ExecuteMethod = context => { throw new Exception("Foo"); };
            this.RollbackMethod = context => { context.Step3_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class SimpleSaga : Saga<SagaContext>
    {
        public SimpleSaga()
        {
            Configure(cfg =>
            {
                cfg.AddStep(new Step1());
                cfg.AddStep(new Step2());
                cfg.AddStep(new Step3());
            });
        }
    }

    public class SimpleSagaWithRollback : Saga<SagaContext>
    {
        public SimpleSagaWithRollback()
        {
            Configure(cfg =>
            {
                cfg.AddStep(new Step1());
                cfg.AddStep(new Step2());
                cfg.AddStep(new Step3WithException());
            });
        }
    }

    public class UOWStep1 : UnitOfWorkStep<SagaContext>
    {
        public UOWStep1()
        {
            this.ExecuteMethod = (context, uow) => { context.Step1_Invoked = true; return Task.FromResult(context); };
            this.RollbackMethod = (context, uow) => { context.Step1_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class UOWStep2 : UnitOfWorkStep<SagaContext>
    {
        public UOWStep2()
        {
            this.ExecuteMethod = (context, uow) => { context.Step2_Invoked = true; return Task.FromResult(context); };
            this.RollbackMethod = (context, uow) => { context.Step2_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class UOWStep3 : UnitOfWorkStep<SagaContext>
    {
        public UOWStep3()
        {
            this.ExecuteMethod = (context, uow) => { context.Step3_Invoked = true; return Task.FromResult(context); };
            this.RollbackMethod = (context, uow) => { context.Step3_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class UOWStep3WithException : UnitOfWorkStep<SagaContext>
    {
        public UOWStep3WithException()
        {
            ExecuteMethod = (context, uow) => { throw new Exception("Foo"); };
            this.RollbackMethod = (context, uow) => { context.Step3_Rollback = true; return Task.FromResult(context); };
        }
    }

    public class SimpleUOWSaga : UnitOfWorkSaga<SagaContext>
    {
        public SimpleUOWSaga(IUnitOfWorkFactory factory) : base(factory)
        {
            Configure(cfg =>
            {
                cfg.AddStep(new UOWStep1());
                cfg.AddStep(new UOWStep2());
                cfg.AddStep(new UOWStep3());
            });
        }
    }

    public class SimpleUOWSagaWithRollback : UnitOfWorkSaga<SagaContext>
    {
        public SimpleUOWSagaWithRollback(IUnitOfWorkFactory factory) : base(factory)
        {
            Configure(cfg =>
            {
                cfg.AddStep(new UOWStep1());
                cfg.AddStep(new UOWStep2());
                cfg.AddStep(new UOWStep3WithException());
            });
        }
    }
    #endregion
}
