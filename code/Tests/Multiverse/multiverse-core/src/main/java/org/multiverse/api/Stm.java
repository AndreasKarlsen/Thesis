package org.multiverse.api;

import org.multiverse.api.collections.TxnCollectionsFactory;
import org.multiverse.api.references.TxnRefFactory;
import org.multiverse.api.references.TxnRefFactoryBuilder;

/**
 * The main interface for software transactional memory. The main tasks are done by the following structures:
 * <ol>
 * <li>{@link TxnObject}: the structure where state and identity are separated and where state change
 * is coordinated through a transaction. An example of the TxnObject is the {@link org.multiverse.api.references.TxnRef},
 * but it could just as easily by a more complex transactional datastructure that is enhanced by instrumentation.
 * </li>
 * <li>{@link Txn}: responsible for making sure that all changes on transactionalobjects are atomic, isolated and consistent.
 * </li>
 * <li>{@link TxnExecutor}: responsible for starting/committing/aborting/retrying transactions. The TxnExecutor executes an
 * {@link org.multiverse.api.callables.TxnCallable} (there are different tastes for return values). The TxnCallable contains
 * the logic that needs to be executed atomic, isolated and consistent.
 * </li>
 * </ol>
 *
 * <h3>Pluggability</h3>
 *
 * <p>The Stm interface provides a mechanism to separate the contract from the implementation. So it is possible to change the
 * Stm implementation without changing the code that uses it. The idea is that for example a TL2 (MVCC) based implementation can
 * be replaced by a Sky-STM or a lock based STM. Of course every Stm implementation will have its strong and weak
 * spots.
 *
 * <p>All functionality like TxnExecutors, Refs, Txn etc can be customized by providing a custom implementation of the
 * factory/builder interfaces:
 * <ol>
 * <li>{@link org.multiverse.api.references.TxnRefFactoryBuilder} a builder for creating {@link org.multiverse.api.references.TxnRefFactory}</li>
 * <li>{@link TxnFactoryBuilder} a builder for creating an {@link TxnExecutor}/{@link Txn}.
 * <li>{@link org.multiverse.api.collections.TxnCollectionsFactory} a factory for creating transactional collections</li>
 * </ol>
 *
 * <h3>Multiple Stm instances</h3>
 *
 * <p>It is important that an TxnObject only is used within a single Stm. If it is 'shared' between different
 * stm instances, isolation problems could happen. This can be caused by the fact that different stm instances
 * probably use different clocks or completely different mechanisms for preventing isolation problems. It depends on the
 * implementation if any checking is done (the GammaStm does check if there is a conflict).
 *
 * <h3>Thread safe</h3>
 * All methods on the Stm are of course thread safe.
 *
 * @author Peter Veentjer.
 */
public interface Stm {

    /**
     * Gets the {@link TxnFactoryBuilder} that needs to be used to execute a {@link Txn} created by this Stm.
     * See the {@link TxnFactoryBuilder} for more info. The TxnFactoryBuilder also is responsible for creating
     * the TxnExecutor since the Txn and TxnExecutor can be tightly coupled.
     *
     * @return the TxnFactoryBuilder that is used to execute transactions on this Stm.
     */
    TxnFactoryBuilder newTxnFactoryBuilder();

    /**
     * Starts a default Txn that is useful for testing/experimentation purposes. This method is purely for easy to use access,
     * but doesn't provide any configuration options. See the {@link #newTxnFactoryBuilder()} for something more configurable.
     * In mose cases this is not the method you want to use to manage transactions.
     *
     * <p>Transactions returned by this method are not speculative.
     *
     * @return the new default Txn.
     */
    Txn newDefaultTxn();

    /**
     * Returns the default {@link TxnExecutor} that is useful for testing/experimentation purposes.
     * This method is purely for easy to use access, but it doesn't provide any configuration options.
     * See the {@link #newTxnFactoryBuilder()} for something more configurable.
     *
     * <p>Transactions used in this Block are not speculative.
     *
     * @return the default TxnExecutor.
     */
    TxnExecutor getDefaultTxnExecutor();

    /**
     * Creates an OrElseBlock.
     *
     * @return the created OrElseBlock.
     */
    OrElseBlock newOrElseBlock();

    /**
     * Returns the default {@link org.multiverse.api.references.TxnRefFactory} that can be used for easy and cheap access to a reference factory
     * instead of setting one up through the {@link org.multiverse.api.references.TxnRefFactoryBuilder}.
     *
     * @return the default TxnRefFactory.
     */
    TxnRefFactory getDefaultRefFactory();

    /**
     * Gets the {@link org.multiverse.api.references.TxnRefFactoryBuilder}.
     *
     * @return the TxnRefFactoryBuilder.
     */
    TxnRefFactoryBuilder getTxRefFactoryBuilder();

    /**
     * Gets the default {@link org.multiverse.api.collections.TxnCollectionsFactory}.
     *
     * @return the default {@link org.multiverse.api.collections.TxnCollectionsFactory}.
     */
    TxnCollectionsFactory getDefaultTxnCollectionFactory();
}
