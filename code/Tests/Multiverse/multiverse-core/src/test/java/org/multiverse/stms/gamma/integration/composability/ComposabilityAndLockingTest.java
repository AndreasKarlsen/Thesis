package org.multiverse.stms.gamma.integration.composability;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.LockMode;
import org.multiverse.api.StmUtils;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;
import org.multiverse.api.references.TxnLong;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;

import static org.junit.Assert.assertEquals;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class ComposabilityAndLockingTest {

    private GammaStm stm;

    @Before
    public void setUp() {
        stm = (GammaStm) getGlobalStmInstance();
        clearThreadLocalTxn();
    }

    @Test
    public void whenEnsuredInOuter_thenCanSafelyBeEnsuredInInner() {
        final int initialValue = 10;
        final TxnLong ref = new GammaTxnLong(stm, initialValue);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                ref.getLock().acquire(LockMode.Write);

                StmUtils.atomic(new TxnVoidCallable() {
                    @Override
                    public void call(Txn tx) throws Exception {
                        ref.getLock().acquire(LockMode.Write);
                        assertEquals(LockMode.Write, ref.getLock().getLockMode());
                    }
                });
            }
        });

        assertEquals(LockMode.None, ref.getLock().atomicGetLockMode());
        assertEquals(initialValue, ref.atomicGet());
    }

    @Test
    public void whenEnsuredInOuter_thenCanSafelyBePrivatizedInInner() {
        final int initialValue = 10;
        final TxnLong ref = new GammaTxnLong(stm, initialValue);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                ref.getLock().acquire(LockMode.Write);

                StmUtils.atomic(new TxnVoidCallable() {
                    @Override
                    public void call(Txn tx) throws Exception {
                        ref.getLock().acquire(LockMode.Exclusive);
                        assertEquals(LockMode.Exclusive, ref.getLock().getLockMode());
                    }
                });
            }
        });

        assertEquals(LockMode.None, ref.getLock().atomicGetLockMode());
        assertEquals(initialValue, ref.atomicGet());
    }

    @Test
    public void whenPrivatizedInOuter_thenCanSafelyBeEnsuredInInner() {
        final int initialValue = 10;
        final TxnLong ref = new GammaTxnLong(stm, initialValue);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                ref.getLock().acquire(LockMode.Exclusive);

                StmUtils.atomic(new TxnVoidCallable() {
                    @Override
                    public void call(Txn tx) throws Exception {
                        ref.getLock().acquire(LockMode.Write);
                        assertEquals(LockMode.Exclusive, ref.getLock().getLockMode());
                    }
                });
            }
        });

        assertEquals(LockMode.None, ref.getLock().atomicGetLockMode());
        assertEquals(initialValue, ref.atomicGet());
    }

    @Test
    public void whenPrivatizedInOuter_thenCanSafelyBePrivatizedInInner() {
        final int initialValue = 10;
        final TxnLong ref = new GammaTxnLong(stm, initialValue);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                ref.getLock().acquire(LockMode.Exclusive);

                StmUtils.atomic(new TxnVoidCallable() {
                    @Override
                    public void call(Txn tx) throws Exception {
                        ref.getLock().acquire(LockMode.Exclusive);
                        assertEquals(LockMode.Exclusive, ref.getLock().getLockMode());
                    }
                });
            }
        });

        assertEquals(LockMode.None, ref.getLock().atomicGetLockMode());
        assertEquals(initialValue, ref.atomicGet());
    }
}
