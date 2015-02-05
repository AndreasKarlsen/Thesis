package org.multiverse.stms.gamma.integration.classic;

import org.junit.Before;
import org.multiverse.TestThread;
import org.multiverse.api.Txn;
import org.multiverse.api.TxnExecutor;
import org.multiverse.api.callables.TxnVoidCallable;
import org.multiverse.api.references.TxnBoolean;
import org.multiverse.api.references.TxnRefFactory;
import org.multiverse.stms.gamma.GammaConstants;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.GammaStmConfig;

import static org.junit.Assert.assertFalse;
import static org.multiverse.TestUtils.*;
import static org.multiverse.api.StmUtils.retry;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;


/**
 * http://en.wikipedia.org/wiki/Dining_philosophers_problem
 */
public abstract class DiningPhilosophers_AbstractTest implements GammaConstants {

    private int philosopherCount = 10;
    private volatile boolean stop;

    private TxnBoolean[] forks;
    protected GammaStm stm;
    private TxnRefFactory refFactory;

    @Before
    public void setUp() {
        clearThreadLocalTxn();
        GammaStmConfig config = new GammaStmConfig();
        //config.backoffPolicy = new SpinningBackoffPolicy();
        stm = new GammaStm(config);
        refFactory = stm.getTxRefFactoryBuilder().build();
        stop = false;
    }

    protected abstract TxnExecutor newTakeForksBlock();

    protected abstract TxnExecutor newReleaseForksBlock();

    public void run() {
        createForks();

        PhilosopherThread[] philosopherThreads = createPhilosopherThreads();
        startAll(philosopherThreads);

        sleepMs(getStressTestDurationMs(30 * 1000));

        stop = true;
        joinAll(philosopherThreads);

        assertAllForksHaveReturned();

        for (PhilosopherThread philosopherThread : philosopherThreads) {
            System.out.printf("%s ate %s times\n",
                    philosopherThread.getName(), philosopherThread.eatCount);
        }
    }

    public void assertAllForksHaveReturned() {
        for (TxnBoolean fork : forks) {
            assertFalse(fork.atomicGet());
        }
    }

    public PhilosopherThread[] createPhilosopherThreads() {
        PhilosopherThread[] threads = new PhilosopherThread[philosopherCount];
        for (int k = 0; k < philosopherCount; k++) {
            TxnBoolean leftFork = forks[k];
            TxnBoolean rightFork = k == philosopherCount - 1 ? forks[0] : forks[k + 1];
            threads[k] = new PhilosopherThread(k, leftFork, rightFork);
        }
        return threads;
    }

    public void createForks() {
        forks = new TxnBoolean[philosopherCount];
        for (int k = 0; k < forks.length; k++) {
            forks[k] = refFactory.newTxnBoolean(false);
        }
    }

    class PhilosopherThread extends TestThread {
        private int eatCount = 0;
        private final TxnBoolean leftFork;
        private final TxnBoolean rightFork;
        private final TxnExecutor releaseForksBlock = newReleaseForksBlock();
        private final TxnExecutor takeForksBlock = newTakeForksBlock();

        PhilosopherThread(int id, TxnBoolean leftFork, TxnBoolean rightFork) {
            super("PhilosopherThread-" + id);
            this.leftFork = leftFork;
            this.rightFork = rightFork;
        }

        @Override
        public void doRun() {
            while (!stop) {
                eatCount++;
                if (eatCount % 100 == 0) {
                    System.out.printf("%s at %s\n", getName(), eatCount);
                }
                eat();
                //   sleepMs(5);
            }
        }

        public void eat() {
            takeForks();
            stuffHole();
            releaseForks();
        }

        private void stuffHole() {
            //simulate the eating
            sleepRandomMs(50);
        }

        public void releaseForks() {
            releaseForksBlock.execute(new TxnVoidCallable() {
                @Override
                public void call(Txn tx) throws Exception {
                    leftFork.set(false);
                    rightFork.set(false);

                }
            });
        }

        public void takeForks() {
            takeForksBlock.execute(new TxnVoidCallable() {
                @Override
                public void call(Txn tx) throws Exception {
                    if (leftFork.get() || rightFork.get()) {
                        retry();
                    }

                    leftFork.set(true);
                    rightFork.set(true);
                }
            });
        }
    }


}
