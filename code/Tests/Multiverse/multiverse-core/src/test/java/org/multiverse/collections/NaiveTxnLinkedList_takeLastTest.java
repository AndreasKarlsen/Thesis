package org.multiverse.collections;

import org.junit.Before;
import org.junit.Ignore;
import org.junit.Test;
import org.multiverse.api.Stm;
import org.multiverse.api.StmUtils;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNull;
import static org.junit.Assert.assertSame;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class NaiveTxnLinkedList_takeLastTest {

    private Stm stm;
    private NaiveTxnLinkedList<String> list;

    @Before
    public void setUp() {
        stm = getGlobalStmInstance();
        clearThreadLocalTxn();
        list = new NaiveTxnLinkedList<String>(stm);
    }

    @Test
    @Ignore
    public void whenEmpty() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                String item = list.takeLast();

                assertNull(item);
                assertEquals("[]", list.toString());
                assertEquals(0, list.size());
            }
        });
    }

    @Test
    public void whenMultipleItems() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                list.offerLast("1");
                list.offerLast("2");
                list.offerLast("3");

                String found = list.takeLast();

                assertEquals("3", found);
                assertEquals("[1, 2]", list.toString());
                assertEquals(2, list.size());
            }
        });
    }

    @Test
    public void whenSingleItem() {
        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                String item = "1";
                list.put(item);

                String found = list.takeLast();

                assertSame(item, found);
                assertEquals("[]", list.toString());
                assertEquals(0, list.size());
            }
        });
    }

}
