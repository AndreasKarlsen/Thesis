package org.multiverse.collections;

import org.junit.Before;
import org.junit.Test;
import org.multiverse.api.Stm;
import org.multiverse.api.StmUtils;
import org.multiverse.api.Txn;
import org.multiverse.api.callables.TxnVoidCallable;

import java.util.Arrays;
import java.util.LinkedList;
import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.fail;
import static org.multiverse.api.GlobalStmInstance.getGlobalStmInstance;
import static org.multiverse.api.TxnThreadLocal.clearThreadLocalTxn;

public class NaiveTxnLinkedList_addAllTest {

    private Stm stm;

    @Before
    public void setUp() {
        stm = getGlobalStmInstance();
        clearThreadLocalTxn();
    }

    @Test
    public void whenNullCollectionAdded_thenNullPointerException() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                try {
                    list.addAll(null);
                    fail();
                } catch (NullPointerException expected) {
                }

                assertEquals("[]", list.toString());
            }
        });
    }

    @Test
    public void whenOneOfItemsIsNull() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        try {
            StmUtils.atomic(new TxnVoidCallable() {
                @Override
                public void call(Txn tx) throws Exception {
                    List<String> c = Arrays.asList("a", "b", null, "d");

                    try {
                        list.addAll(c);
                        fail();
                    } catch (NullPointerException expected) {
                    }

                    assertEquals("[a, b]", list.toString());

                    throw new NullPointerException();
                }
            });
            fail();
        } catch (NullPointerException expected) {
        }

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                assertEquals("[]", list.toString());
                assertEquals(0, list.size());
            }
        });
    }

    @Test
    public void whenNonEmptyListAndEmptyCollection() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                list.add("1");
                list.add("2");
                list.addAll(new LinkedList<String>());

                assertEquals("[1, 2]", list.toString());
                assertEquals(2, list.size());
            }
        });
    }

    @Test
    public void whenBothEmpty() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                list.addAll(new LinkedList<String>());

                assertEquals("[]", list.toString());
                assertEquals(0, list.size());
            }
        });
    }

    @Test
    public void whenEmptyListEmptyAndNonEmptyCollection() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                List<String> c = Arrays.asList("1", "2");

                list.addAll(c);

                assertEquals("[1, 2]", list.toString());
                assertEquals(2, list.size());
            }
        });
    }

    @Test
    public void whenBothNonEmpty() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                List<String> c = Arrays.asList("2", "1");

                list.add("4");
                list.add("3");
                list.addAll(c);

                assertEquals("[4, 3, 2, 1]", list.toString());
                assertEquals(4, list.size());
            }
        });
    }

    @Test
    public void whenCapacityExceeded() {
        final NaiveTxnLinkedList<String> list = new NaiveTxnLinkedList<String>(stm, 2);

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                list.add("1");
            }
        });

        try {
            StmUtils.atomic(new TxnVoidCallable() {
                @Override
                public void call(Txn tx) throws Exception {
                    List<String> c = Arrays.asList("2", "3");

                    try {
                        list.addAll(c);
                        fail();
                    } catch (IllegalStateException expected) {

                    }

                    assertEquals("[1, 2]", list.toString());
                    assertEquals(2, list.size());
                    throw new IllegalStateException();
                }
            });
            fail();
        } catch (IllegalStateException expected) {

        }

        StmUtils.atomic(new TxnVoidCallable() {
            @Override
            public void call(Txn tx) throws Exception {
                assertEquals(1, list.size());
                assertEquals("[1]", list.toString());
            }
        });
    }
}
