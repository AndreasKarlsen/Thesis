package org.multiverse.stms.gamma.integration.isolation;

import org.junit.Test;
import org.multiverse.api.TxnExecutor;
import org.multiverse.api.LockMode;
import org.multiverse.stms.gamma.LeanGammaTxnExecutor;
import org.multiverse.stms.gamma.transactions.GammaTxnConfig;
import org.multiverse.stms.gamma.transactions.fat.FatFixedLengthGammaTxnFactory;

public class MoneyTransfer_FatFixedLengthGammaTxn_StressTest extends MoneyTransfer_AbstractTest {

    private LockMode lockMode;
    private int accountCount;

    @Test
    public void when10AccountsAnd2ThreadsAndOptimistic() {
        lockMode = LockMode.None;
        accountCount = 10;
        run(accountCount, 2);
    }

    @Test
    public void when10AccountsAnd2ThreadsAndPessimistic() {
        lockMode = LockMode.Exclusive;
        accountCount = 10;
        run(accountCount, 2);
    }

    @Test
    public void when100AccountAnd10ThreadsAndOptimistic() {
        lockMode = LockMode.None;
        accountCount = 100;
        run(accountCount, 10);
    }

    @Test
    public void when100AccountAnd10ThreadsAndPessimistic() {
        lockMode = LockMode.Exclusive;
        accountCount = 100;
        run(accountCount, 10);
    }


    @Test
    public void when30AccountsAnd30ThreadsAndOptimistic() {
        lockMode = LockMode.None;
        accountCount = 30;
        run(accountCount, 30);
    }

    @Override
    protected TxnExecutor newTxnExecutor() {
        GammaTxnConfig config = new GammaTxnConfig(stm, accountCount)
                .setReadLockMode(lockMode);
        return new LeanGammaTxnExecutor(new FatFixedLengthGammaTxnFactory(config));
    }
}
