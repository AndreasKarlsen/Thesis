package org.multiverse.stms.gamma.transactionalobjects.txnlong;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.Parameterized;
import org.multiverse.api.TxnFactory;
import org.multiverse.api.exceptions.DeadTxnException;
import org.multiverse.api.exceptions.PreparedTxnException;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;
import org.multiverse.stms.gamma.transactions.GammaTxn;
import org.multiverse.stms.gamma.transactions.GammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatFixedLengthGammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatMonoGammaTxnFactory;
import org.multiverse.stms.gamma.transactions.fat.FatVariableLengthGammaTxnFactory;

import java.util.Collection;

import static java.util.Arrays.asList;
import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;
import static org.multiverse.TestUtils.assertIsAborted;
import static org.multiverse.TestUtils.assertIsCommitted;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;
import static org.multiverse.stms.gamma.GammaTestUtils.assertVersionAndValue;

@RunWith(Parameterized.class)
public class GammaTxnLong_set2Test {

    private final GammaTxnFactory transactionFactory;
    private final GammaStm stm;

    public GammaTxnLong_set2Test(GammaTxnFactory transactionFactory) {
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
    public void whenNullTransaction_thenNullPointerException() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);
        long version = ref.getVersion();

        try {
            ref.set(null, 20);
            fail();
        } catch (NullPointerException expected) {
        }

        assertVersionAndValue(ref, version, 10);
    }

    @Test
    public void whenPreparedTransaction_thenPreparedTxnException() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        tx.prepare();
        try {
            ref.set(tx, 20);
            fail();
        } catch (PreparedTxnException expected) {
        }

        assertIsAborted(tx);
        assertVersionAndValue(ref, version, 10);
    }

    @Test
    public void whenAborted_thenDeadTxnException() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        tx.abort();
        try {
            ref.set(tx, 20);
            fail();
        } catch (DeadTxnException expected) {
        }

        assertIsAborted(tx);
        assertVersionAndValue(ref, version, 10);
    }

    @Test
    public void whenCommitted_thenDeadTxnException() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);
        long version = ref.getVersion();

        GammaTxn tx = transactionFactory.newTxn();
        tx.commit();
        try {
            ref.set(tx, 20);
            fail();
        } catch (DeadTxnException expected) {
        }

        assertIsCommitted(tx);
        assertVersionAndValue(ref, version, 10);
    }

    @Test
    public void test() {
        GammaTxnLong ref = new GammaTxnLong(stm, 10);

        GammaTxn tx = transactionFactory.newTxn();
        long value = ref.get(tx);

        assertEquals(10, value);
    }
}
