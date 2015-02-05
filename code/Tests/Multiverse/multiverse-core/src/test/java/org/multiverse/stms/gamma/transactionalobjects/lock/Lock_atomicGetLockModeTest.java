package org.multiverse.stms.gamma.transactionalobjects.lock;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.LockMode;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnLong;
import org.multiverse.stms.gamma.transactions.GammaTxn;

import static org.junit.Assert.assertEquals;

public class Lock_atomicGetLockModeTest {

    private GammaStm stm;

    @Before
    public void setUp() {
        stm = new GammaStm();
    }

    @Test
    public void test() {
        test(LockMode.None);
        test(LockMode.Read);
        test(LockMode.Write);
        test(LockMode.Exclusive);
    }

    public void test(LockMode lockMode){
         GammaTxnLong ref = new GammaTxnLong(stm);
        GammaTxn tx = stm.newDefaultTxn();
        ref.getLock().acquire(tx, lockMode);

        LockMode result = ref.atomicGetLockMode();
        assertEquals(lockMode, result);
    }
}
