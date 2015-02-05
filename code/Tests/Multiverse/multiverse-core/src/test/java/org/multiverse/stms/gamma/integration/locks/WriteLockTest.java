package org.multiverse.stms.gamma.integration.locks;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.LockMode;
import org.multiverse.api.exceptions.ReadWriteConflict;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;
import org.multiverse.stms.gamma.transactions.GammaTxn;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;
import static org.multiverse.TestUtils.*;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;
import static org.multiverse.stms.gamma.GammaTestUtils.*;

public class WriteLockTest {

    private GammaStm stm;

    @Before
    public void setUp() {
        stm = (GammaStm) getGlobalStmInstance();
        clearThreadLocalTxn();
    }

    @Test
    public void whenUnlocked() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Write);

        assertIsActive(tx);
        assertRefHasWriteLock(ref, tx);
    }

    @Test
    public void whenReadLockAlreadyAcquiredByOther_thenWriteLockFails() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Read);

        GammaTxn tx = stm.newDefaultTxn();
        try {
            ref.getLock().acquire(tx, LockMode.Exclusive);
            fail();
        } catch (ReadWriteConflict expected) {

        }

        assertIsAborted(tx);
        assertRefHasReadLock(ref, otherTx);
        assertReadLockCount(ref, 1);
    }

    @Test
    public void whenWriteLockAlreadyAcquiredOther_thenWriteLockFails() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Write);

        GammaTxn tx = stm.newDefaultTxn();
        try {
            ref.getLock().acquire(tx, LockMode.Write);
            fail();
        } catch (ReadWriteConflict expected) {

        }

        assertIsAborted(tx);
        assertRefHasWriteLock(ref, otherTx);
    }

    @Test
    public void whenExclusiveLockAlreadyAcquiredByOther_thenWriteLockFails() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Exclusive);

        GammaTxn tx = stm.newDefaultTxn();
        try {
            ref.getLock().acquire(tx, LockMode.Write);
            fail();
        } catch (ReadWriteConflict expected) {

        }

        assertIsAborted(tx);
        assertRefHasExclusiveLock(ref, otherTx);
    }


    @Test
    public void whenWriteLockAcquiredByOther_thenReadStillAllowed() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Write);

        GammaTxn tx = stm.newDefaultTxn();

        long result = ref.get(tx);

        assertEquals(5, result);
        assertIsActive(tx);
        assertRefHasWriteLock(ref, otherTx);
    }

    @Test
    public void whenPreviouslyReadByOtherThread_thenNoProblems() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);

        GammaTxn tx = stm.newDefaultTxn();
        ref.get(tx);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Write);

        long result = ref.get(tx);

        assertEquals(10, result);
        assertIsActive(tx);
        assertRefHasWriteLock(ref, otherTx);
    }

    @Test
    public void whenPreviouslyReadByOtherTransaction_thenWriteSuccessButCommitFails() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);

        GammaTxn tx = stm.newDefaultTxn();
        ref.get(tx);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Write);

        ref.set(tx, 100);

        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertIsAborted(tx);
        assertRefHasWriteLock(ref, otherTx);
    }

    @Test
    public void whenWriteLockAcquired_thenWriteAllowedButCommitFails() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Write);

        GammaTxn tx = stm.newDefaultTxn();
        ref.set(tx, 100);

        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertIsAborted(tx);
        assertRefHasWriteLock(ref, otherTx);
    }

    @Test
    public void whenReadLockAlreadyAcquiredBySelf_thenWriteLockAcquired() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Read);
        ref.getLock().acquire(tx, LockMode.Write);

        assertIsActive(tx);
        assertRefHasWriteLock(ref, tx);
    }

    @Test
    public void whenReadLockAlsoAcquiredByOther_thenWriteLockFails() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn otherTx = stm.newDefaultTxn();
        ref.getLock().acquire(otherTx, LockMode.Read);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Read);

        try {
            ref.getLock().acquire(tx, LockMode.Write);
            fail();
        } catch (ReadWriteConflict expected) {

        }

        assertIsAborted(tx);
        assertRefHasReadLock(ref, otherTx);
        assertReadLockCount(ref, 1);
    }

    @Test
    public void whenWriteLockAlreadyAcquiredBySelf_thenSuccess() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Write);
        ref.getLock().acquire(tx, LockMode.Write);

        assertIsActive(tx);
        assertRefHasWriteLock(ref, tx);
    }

    @Test
    public void whenExclusiveLockAlreadyAcquiredBySelf_thenExclusiveLockRemains() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Exclusive);
        ref.getLock().acquire(tx, LockMode.Write);

        assertIsActive(tx);
        assertRefHasExclusiveLock(ref, tx);
    }

    @Test
    public void whenTransactionCommits_thenWriteLockIsReleased() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Write);
        tx.commit();

        assertIsCommitted(tx);
        assertRefHasNoLocks(ref);
    }

    @Test
    public void whenTransactionIsPrepared_thenWriteLockRemains() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Write);
        tx.prepare();

        assertIsPrepared(tx);
        assertRefHasWriteLock(ref, tx);
    }

    @Test
    public void whenTransactionAborts_thenWriteLockReleased() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, LockMode.Write);
        tx.abort();

        assertIsAborted(tx);
        assertRefHasNoLocks(ref);
    }

    @Test
    public void whenReadConflict_thenAcquireWriteLockFails() {
        GammaTxnLong ref = new GammaTxnLong(stm, 5);

        GammaTxn tx = stm.newDefaultTxn();
        ref.get(tx);

        ref.atomicIncrementAndGet(1);

        try {
            ref.getLock().acquire(tx, LockMode.Write);
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertRefHasNoLocks(ref);
        assertIsAborted(tx);
    }
}
