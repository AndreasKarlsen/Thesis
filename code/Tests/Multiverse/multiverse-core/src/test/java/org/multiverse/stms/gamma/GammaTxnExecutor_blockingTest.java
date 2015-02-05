package org.multiverse.stms.gamma;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.TestThread;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;
import org.multiverse.stms.gamma.transactionalobjects.Tranlocal;
import org.multiverse.stms.gamma.transactions.GammaTxn;

import static org.junit.Assert.assertEquals;
import static org.multiverse.TestUtils.*;
import static org.multiverse.api.StmUtils.retry;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class GammaTxnExecutor_blockingTest {

    private GammaStm stm;

    @Before
    public void setUp() {
        stm = new GammaStm();
        clearThreadLocalTxn();
    }

    @Test
    public void test() {
        final GammaTxnLong ref = new GammaTxnLong(stm);

        WaitThread t = new WaitThread(ref);
        t.start();

        sleepMs(1000);
        assertAlive(t);

        stm.getDefaultTxnExecutor().execute(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                GammaTxn btx = (GammaTxn) tx;
                Tranlocal write = ref.openForWrite(btx, LOCKMODE_NONE);
                write.long_value = 1;
            }
        });

        joinAll(t);
        assertEquals(2, ref.atomicGet());
    }

    class WaitThread extends TestThread {
        final GammaTxnLong ref;

        public WaitThread(GammaTxnLong ref) {
            this.ref = ref;
        }

        @Override
        public void doRun() throws Exception {
            stm.getDefaultTxnExecutor().execute(new TxnVoidCallable() {
                @Override
                public void call(Txn tx) throws Exception {
                    GammaTxn btx = (GammaTxn) tx;
                    Tranlocal write = ref.openForWrite(btx, LOCKMODE_NONE);
                    if (write.long_value == 0) {
                        retry();
                    }

                    write.long_value++;
                }
            });
        }
    }
}
