package org.multiverse.stms.gamma.integration.isolation;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.exceptions.ReadWriteConflict;
import org.multiverse.stms.gamma.GammaConstants;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;
import org.multiverse.stms.gamma.transactionalobjects.Tranlocal;
import org.multiverse.stms.gamma.transactions.GammaTxn;

import static org.junit.Assert.fail;
import static org.multiverse.TestUtils.assertIsAborted;
import static org.multiverse.TestUtils.assertIsCommitted;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;
import static org.multiverse.stms.gamma.GammaTestUtils.assertVersionAndValue;

public class WriteConflictTest implements GammaConstants {

    private GammaStm stm;

    @Before
    public void setUp() {
        clearThreadLocalTxn();
        stm = (GammaStm) getGlobalStmInstance();
    }

    @After
    public void tearDown() {
        clearThreadLocalTxn();
    }

    @Test
    public void whenDirtyCheckAndChange_ThenWriteConflict() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn tx = stm.newTxnFactoryBuilder()
                .setSpeculative(false)
                .setDirtyCheckEnabled(true)
                .newTransactionFactory()
                .newTxn();
        Tranlocal write = ref.openForWrite(tx, LOCKMODE_NONE);
        write.long_value++;

        ref.atomicIncrementAndGet(1);
        long version = ref.getVersion();

        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {

        }

        assertIsAborted(tx);
        assertVersionAndValue(ref, version, 1);
    }

    @Test
    public void whenDirtyCheckAndNoChange_ThenNoWriteConflict() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn tx = stm.newTxnFactoryBuilder()
                .setSpeculative(false)
                .setDirtyCheckEnabled(true)
                .newTransactionFactory()
                .newTxn();

        ref.openForWrite(tx, LOCKMODE_NONE);

        ref.atomicIncrementAndGet(1);
        long version = ref.getVersion();

        tx.commit();

        assertIsCommitted(tx);
        assertVersionAndValue(ref, version, 1);
    }


    @Test
    public void whenNoDirtyCheckAndChange_ThenWriteConflict() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn tx = stm.newTxnFactoryBuilder()
                .setSpeculative(false)
                .setDirtyCheckEnabled(false)
                .newTransactionFactory()
                .newTxn();
        Tranlocal write = ref.openForWrite(tx, LOCKMODE_NONE);
        write.long_value++;

        ref.atomicIncrementAndGet(1);
        long version = ref.getVersion();

        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertIsAborted(tx);
        assertVersionAndValue(ref, version, 1);
    }

    @Test
    public void whenNoDirtyCheckAndNoChange_ThenWriteConflict() {
        GammaTxnLong ref = new GammaTxnLong(stm);

        GammaTxn tx = stm.newTxnFactoryBuilder()
                .setSpeculative(false)
                .setDirtyCheckEnabled(false)
                .newTransactionFactory()
                .newTxn();
        Tranlocal write = ref.openForWrite(tx, LOCKMODE_NONE);
        write.long_value++;

        ref.atomicIncrementAndGet(1);
        long version = ref.getVersion();

        try {
            tx.commit();
            fail();
        } catch (ReadWriteConflict expected) {
        }

        assertIsAborted(tx);
        assertVersionAndValue(ref, version, 1);
    }
}
