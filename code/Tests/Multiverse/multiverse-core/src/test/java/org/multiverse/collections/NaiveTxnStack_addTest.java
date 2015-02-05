package org.multiverse.collections;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.Stm;
import org.multiverse.api.StmUtils;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;

import static org.junit.Assert.*;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class NaiveTxnStack_addTest {

    private Stm stm;

    @Before
    public void setUp() {
        stm = getGlobalStmInstance();
        clearThreadLocalTxn();
    }

    @Test
    public void whenNullItem_thenNullPointerException() {
        final NaiveTxnStack<String> stack = new NaiveTxnStack<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                try {
                    stack.add(null);
                    fail();
                } catch (NullPointerException expected) {

                }

                assertEquals("[]", stack.toString());
                assertEquals(0, stack.size());
            }
        });
    }

    @Test
    public void whenEmpty() {
        final NaiveTxnStack<String> stack = new NaiveTxnStack<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                boolean result = stack.add("1");

                assertTrue(result);
                assertEquals("[1]", stack.toString());
                assertEquals(1, stack.size());
            }
        });
    }

    @Test
    public void whenNotEmpty() {
        final NaiveTxnStack<String> stack = new NaiveTxnStack<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                stack.add("1");
                boolean result = stack.add("2");

                assertTrue(result);
                assertEquals("[2, 1]", stack.toString());
                assertEquals(2, stack.size());
            }
        });
    }

    @Test
    public void whenFull() {
        final NaiveTxnStack<String> stack = new NaiveTxnStack<String>(stm, 2);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                stack.add("1");
                stack.add("2");

                try {
                    stack.add("3");
                    fail();
                } catch (IllegalStateException expected) {

                }

                assertEquals("[2, 1]", stack.toString());
                assertEquals(2, stack.size());
            }
        });
    }
}
