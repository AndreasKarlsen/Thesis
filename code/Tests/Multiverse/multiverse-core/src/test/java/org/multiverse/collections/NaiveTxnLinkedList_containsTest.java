package org.multiverse.collections;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.Stm;
import org.multiverse.api.StmUtils;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;
import static org.junit.Assert.assertTrue;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class NaiveTxnLinkedList_containsTest {

    private Stm stm;
    private NaiveTxnLinkedList<String> stack;

    @Before
    public void setUp() {
        stm = getGlobalStmInstance();
        clearThreadLocalTxn();
        stack = new NaiveTxnLinkedList<String>(stm);
    }

    @Test
    public void whenNullItem() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                stack.add("1");
                stack.add("2");
                boolean result = stack.contains(null);
                assertFalse(result);
                assertEquals("[1, 2]", stack.toString());
            }
        });
    }

    @Test
    public void whenListStack() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                boolean result = stack.contains("foo");

                assertFalse(result);
                assertEquals("[]", stack.toString());
            }
        });
    }

    @Test
    public void whenListDoesntContainItem() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                stack.add("1");
                stack.add("2");
                stack.add("3");
                stack.add("4");

                boolean result = stack.contains("b");

                assertFalse(result);
                assertEquals("[1, 2, 3, 4]", stack.toString());
            }
        });
    }

    @Test
    public void whenContainsItem() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                stack.add("1");
                stack.add("2");
                stack.add("3");
                stack.add("4");

                boolean result = stack.contains("3");

                assertTrue(result);
                assertEquals("[1, 2, 3, 4]", stack.toString());
            }
        });
    }
}
