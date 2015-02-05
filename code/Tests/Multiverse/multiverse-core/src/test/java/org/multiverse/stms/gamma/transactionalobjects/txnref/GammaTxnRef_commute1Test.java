package org.multiverse.stms.gamma.transactionalobjects.txnref;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.multiverse.api.LockMode;
import org.multiverse.api.TxnFactory;
import org.multiverse.api.exceptions.DeadTxnException;
import org.multiverse.api.exceptions.PreparedTxnException;
import org.multiverse.api.exceptions.ReadWriteConflict;
import org.multiverse.api.exceptions.TxnMandatoryException;
import org.multiverse.api.functions.Function;
import org.multiverse.api.functions.Functions;
import org.multiverse.api.functions.LongFunction;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnRef;
import org.multiverse.stms.gamma.transactionalobjects.Tranlocal;
import org.multiverse.stms.gamma.transactions.GammaTxn;
import org.multiverse.stms.gamma.transactions.GammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatFixedLengthGammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatMonoGammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatVariableLengthGammaTxnFactory;

import java.util.Collection;

import static java.util.Arrays.asList;
import static org.junit.Assert.*;
import static org.multiverse.TestUtils.*;
import static org.multiverse.api.TxnThreadLocal.*;
import static org.multiverse.stms.gamma.GammaTestUtils.*;

@RunWith(Parameterized.class)
public class GammaTxnRef_commute1Test {
    private final GammaTxnFactory transactionFactory;
    private final GammaStm stm;

    public GammaTxnRef_commute1Test(GammaTxnFactory transactionFactory) {
        this.transactionFactory = transactionFactory;
        this.stm = transactionFactory.getConfig().getStm();
    }

    @Before
    public void setUp() {
        clearThreadLocalTxn();
    }

    @Parameterized.Parameters
    public static Collection<TxnFactory[]> configs() {
        return asList(
                new TxnFactory[]{new FatVariableLengthGammaTxnFactory(new GammaStm())},
                new TxnFactory[]{new FatFixedLengthGammaTxnFactory(new GammaStm())},
                new TxnFactory[]{new FatMonoGammaTxnFactory(new GammaStm())}
        );
    }

    @Test
    public void whenActiveTransactionAvailable() {
        Long initialValue = 1L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);
        LongFunction function = Functions.incLongFunction(1);
        ref.commute(function);

        Tranlocal commuting = tx.getRefTranlocal(ref);
        assertNotNull(commuting);
        assertTrue(commuting.isCommuting());
        assertFalse(commuting.isRead());
        assertSurplus(ref, 0);
        assertRefHasNoLocks(ref);
        assertEquals(0, commuting.long_value);
        assertIsActive(tx);
        assertSame(tx, getThreadLocalTxn());
        tx.commit();

        assertEquals(new Long(2), ref.atomicGet());
        assertIsCommitted(tx);
        assertSurplus(ref, 0);
        assertRefHasNoLocks(ref);
        assertWriteBiased(ref);
    }

    @Test
    public void whenActiveTransactionAvailableAndNoChange() {
        Long initialValue = 1L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);
        Function<Long> function = Functions.identityFunction();
        ref.commute(function);

        Tranlocal commuting = tx.getRefTranlocal(ref);
        assertNotNull(commuting);
        assertTrue(commuting.isCommuting());
        assertFalse(commuting.isRead());
        assertSurplus(ref, 0);
        assertRefHasNoLocks(ref);
        assertNull(commuting.ref_value);
        assertIsActive(tx);
        assertSame(tx, getThreadLocalTxn());
        tx.commit();

        assertEquals(initialValue, ref.atomicGet());
        assertVersionAndValue(ref, version, initialValue);
        assertIsCommitted(tx);
        assertSurplus(ref, 0);
        assertRefHasNoLocks(ref);
        assertWriteBiased(ref);
    }

    @Test
    public void whenActiveTransactionAvailableAndNullFunction_thenNullPointerException() {
        Long initalValue = 10L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initalValue);
        long version = ref.getVersion();
        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);

        try {
            ref.commute(null);
            fail();
        } catch (NullPointerException expected) {
        }


        assertIsAborted(tx);
        assertSurplus(ref, 0);
        assertWriteBiased(ref);
        assertRefHasNoLocks(ref);
        assertVersionAndValue(ref, version, initalValue);
    }

    @Test
    public void whenNoTransactionAvailable_thenNoTransactionFoundException() {
        long initialValue = 10;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long initialVersion = ref.getVersion();

        LongFunction function = Functions.incLongFunction(1);
        try {
            ref.commute(function);
            fail();
        } catch (TxnMandatoryException expected) {

        }

        assertSurplus(ref, 0);
        assertWriteBiased(ref);
        assertRefHasNoLocks(ref);
        assertVersionAndValue(ref, initialVersion, initialValue);
    }

    @Test
    public void whenCommittedTransactionAvailable_thenDeadTxnException() {
        Long initialValue = 10L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long initialVersion = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);
        tx.commit();

        LongFunction function = Functions.incLongFunction(1);
        try {
            ref.commute(function);
            fail();
        } catch (DeadTxnException expected) {

        }

        assertIsCommitted(tx);
        assertSame(tx, getThreadLocalTxn());
        assertSurplus(ref, 0);
        assertWriteBiased(ref);
        assertRefHasNoLocks(ref);
        assertVersionAndValue(ref, initialVersion, initialValue);
    }

    @Test
    public void whenAbortedTransactionAvailable_thenDeadTxnException() {
        Long initialValue = 10L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long initialVersion = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);
        tx.abort();

        LongFunction function = Functions.incLongFunction(1);
        try {
            ref.commute(function);
            fail();
        } catch (DeadTxnException expected) {

        }

        assertIsAborted(tx);
        assertSame(tx, getThreadLocalTxn());
        assertSurplus(ref, 0);
        assertWriteBiased(ref);
        assertRefHasNoLocks(ref);
        assertVersionAndValue(ref, initialVersion, initialValue);
    }

    @Test
    public void whenPreparedTransactionAvailable_thenPreparedTxnException() {
        Long initialValue = 2L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);
        tx.prepare();

        LongFunction function = Functions.incLongFunction(1);
        try {
            ref.commute(function);
            fail();
        } catch (PreparedTxnException expected) {

        }

        assertIsAborted(tx);
        assertSame(tx, getThreadLocalTxn());
        assertSurplus(ref, 0);
        assertWriteBiased(ref);
        assertRefHasNoLocks(ref);
        assertVersionAndValue(ref, version, initialValue);
        assertEquals(initialValue, ref.atomicGet());
    }

    @Test
    public void whenAlreadyLockedBySelf_thenNoCommute() {
        whenAlreadyLockedBySelf_thenNoCommute(LockMode.Read);
        whenAlreadyLockedBySelf_thenNoCommute(LockMode.Write);
        whenAlreadyLockedBySelf_thenNoCommute(LockMode.Exclusive);
    }

    public void whenAlreadyLockedBySelf_thenNoCommute(LockMode lockMode) {
        Long initialValue = 2L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);

        ref.getLock().acquire(lockMode);
        LongFunction function = Functions.incLongFunction(1);
        ref.commute(function);

        Tranlocal tranlocal = tx.getRefTranlocal(ref);
        assertNotNull(tranlocal);
        assertFalse(tranlocal.isCommuting());
        assertEquals(new Long(3), tranlocal.ref_value);
        assertIsActive(tx);
        assertRefHasLockMode(ref, tx, lockMode.asInt());
        assertSurplus(ref, 1);
        assertWriteBiased(ref);

        tx.commit();

        assertSurplus(ref, 0);
        assertIsCommitted(tx);
        assertRefHasNoLocks(ref);
        assertSame(tx, getThreadLocalTxn());
        assertEquals(new Long(3), ref.atomicGet());
    }

    @Test
    public void whenLockedAcquiredByOther_thenCommuteSucceedsButCommitFails() {
        whenNonExclusiveLockAcquiredByOther_thenCommuteSucceedsButCommitFails(LockMode.Read);
        whenNonExclusiveLockAcquiredByOther_thenCommuteSucceedsButCommitFails(LockMode.Write);
        whenNonExclusiveLockAcquiredByOther_thenCommuteSucceedsButCommitFails(LockMode.Exclusive);
    }

    public void whenNonExclusiveLockAcquiredByOther_thenCommuteSucceedsButCommitFails(LockMode lockMode) {
        Long initialValue = 2L;
        GammaTxnRef<Long> ref = new GammaTxnRef<Long>(stm, initialValue);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        setThreadLocalTxn(tx);

        GammaTxn otherTx = transactionFactory.newTxn();
        ref.getLock().acquire(otherTx, lockMode);

        LongFunction function = Functions.incLongFunction(1);
        ref.commute(function);

        Tranlocal tranlocal = tx.getRefTranlocal(ref);
        assertNotNull(tranlocal);
        assertTrue(tranlocal.isCommuting());
        assertHasCommutingFunctions(tranlocal, function);
        assertIsActive(tx);
        assertRefHasLockMode(ref, otherTx, lockMode.asInt());
        assertSurplus(ref, 1);

        long orecValue = ref.orec;
        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertIsAborted(tx);
        assertSame(tx, getThreadLocalTxn());
        assertOrecValue(ref, orecValue);
        assertVersionAndValue(ref, version, initialValue);
    }
}
