package org.multiverse.stms.gamma.transactions.lean;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.exceptions.SpeculativeConfigurationError;
import org.multiverse.api.functions.Function;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.transactionalobjects.GammaTxnRef;
import org.multiverse.stms.gamma.transactions.GammaTxn;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verifyZeroInteractions;
import static org.multiverse.TestUtils.assertIsAborted;
import static org.multiverse.stms.gamma.GammaTestUtils.*;

public abstract class LeanGammaTxn_commuteTest<T extends GammaTxn> {

    protected GammaStm stm;

    @Before
    public void setUp() {
        stm = new GammaStm();
    }

    public abstract T newTransaction();

    @Test
    public void whenCommute_thenSpeculativeConfigurationError() {
        String initialValue = "foo";
        GammaTxnRef<String> ref = new GammaTxnRef<String>(stm, initialValue);
        long initialVersion = ref.getVersion();

        T tx = newTransaction();
        Function<String> function = mock(Function.class);
        try {
            ref.commute(tx, function);
            fail();
        } catch (SpeculativeConfigurationError expected) {
        }

        verifyZeroInteractions(function);
        assertRefHasNoLocks(ref);
        assertSurplus(ref, 0);
        assertReadonlyCount(ref, 0);
        assertWriteBiased(ref);
        assertIsAborted(tx);
        assertVersionAndValue(ref, initialVersion, initialValue);
        assertTrue(tx.getConfig().speculativeConfiguration.get().commuteDetected);
    }
}
