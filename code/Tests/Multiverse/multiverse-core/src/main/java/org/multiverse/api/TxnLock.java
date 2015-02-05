package org.multiverse.api;

/**
 * The TxnLock provides access to pessimistic behavior of a {@link TxnObject}. STM normally is very optimistic, but
 * in some cases a more pessimistic approach (one with less retries) could be a better fitting solution.
 * <p/>
 * There are 4 different types of lockmodes:
 * <ul>
 * <li><b>LockMode.None:</b> it doesn't do any locking</li>
 * <li><b>LockMode.Read:</b> it allows multiple transactions to acquire the read lock, but transaction acquiring the write-lock
 * or exclusive lock (needed when a transaction wants to commit) is prohibited. If the read lock is acquired by a different
 * transaction, a transaction still is able to read/write, but it isn't allowed to commit the changes (since and exclusive
 * lock is required for that).
 * </li>
 * <li><b>LockMode.Write:</b> it allows only one transaction to acquire the write lock, but unlike a traditional
 * write-lock, reads still are allowed. Normally this would not be acceptable because once the write-lock
 * is acquired, the internals could be modified. But in case of STM, the STM can still provide a consistent view
 * even though the locking transaction has made changes. This essentially is the same behavior you get with the
 * 'select for update' from Oracle. Once the write lock is acquired, other transactions can't acquire the TxnLock.
 * </li>
 * <li><b>LockMode.Exclusive:</b> it allows only one transaction to acquire the commit lock, and readers are not
 * allowed to read anymore. From an isolation perspective, the exclusive lock looks a lot like the synchronized
 * statement (or a {@link java.util.concurrent.locks.ReentrantLock}} where only mutually exclusive access is
 * possible. The exclusive lock normally is used by the STM when it commits.</li>
 * </ul>
 *
 * <h3>TxnLock duration and release</h3>
 *
 * <p>Locks atm are acquired for the remaining duration of the transaction and only will always be automatically
 * released once the transaction commits/aborts. This is essentially the same behavior you get with Oracle once
 * a update/delete/insert is done, or when the record is locked manually by executing the 'select for update'. For
 * this to work it is very important that the {@link org.multiverse.api.exceptions.ControlFlowError} is not caught
 * by the logic executed in an transactional closure, but is caught by the TxnExecutor itself.
 *
 * <h3>Blocking</h3>
 *
 * <p>Atm it isn't possible to block on a lock. What happens is that some spinning is done
 * {@link TxnFactoryBuilder#setSpinCount(int)} and then some retries
 * {@link TxnFactoryBuilder#setMaxRetries(int)} in combination with a backoff
 * {@link TxnFactoryBuilder#setBackoffPolicy(BackoffPolicy)}. In the 0.8 release blocking will
 * probably be added.
 *
 * <h3>Fairness</h3>
 *
 * <p>Atm there is no support for fairness. The big problem with fairness and STM is that the locks are released
 * and the transaction needs to begin again. It could be that a lower priority transaction is faster and acquires
 * the lock again. This is a topic that needs more research and probably will be integrated in the contention
 * management.
 *
 * <h3>TxnLock upgrade</h3>
 *
 * <p>It is possible to upgrade a lock to more strict version, e.g. to upgrade a read-lock to a write-lock.
 * The following upgrades are possible:
 * <ol>
 * <li>LockMode.Read->LockMode.Write: as long as no other transaction has acquired the TxnLock in LockMode.Read</li>
 * <li>LockMode.Read->LockMode.Exclusive: as long as no other transaction has acquired the TxnLock in LockMode.Read</li>
 * <li>LockMode.Write->LockMode.Exclusive: will always succeed</li>
 * </ol>
 * <p>
 * The Txn is allowed to apply a more strict LockMode than the one specified.
 *
 * <h3>TxnLock downgrade</h3>
 *
 * <p>Downgrading locks currently is not possible and downgrade calls are ignored.
 *
 * <h3>Locking scope</h3>
 *
 * <p>Locking can be done on the Txn level (see the {@link TxnFactoryBuilder#setReadLockMode(LockMode)} and
 * {@link TxnFactoryBuilder#setWriteLockMode(LockMode)} where all reads or all writes (to do a write also a read
 * is needed) are locked automatically. It can also be done on the reference level using
 * getAndLock/setAndLock/getAndSetAndLock methods or by accessing the {@link TxnObject#getLock()}.
 *
 * <h3>TxnLock escalation</h3>
 *
 * <p>In traditional lock based databases, managing locks in memory can be quite expensive. That is one of the reason why
 * different TxnLock granularities are used (record level, page level, table level for example). To prevent managing too many
 * locks, some databases apply lock escalation so that multiple low granularity locks are upgraded to a single higher granular
 * lock. The problem with lock escalations is that the system could be subject to lock contention and to deadlocks.
 *
 * <p>The GammaStm (the main STM implementation) doesn't use lock escalation, but keeps on managing locks on the transactional object
 * (ref) level.
 *
 * <h3>Deadlocks</h3>
 *
 * <p>2 Ingredients are needed for a deadlock:
 * <ol>
 * <li>Transactions acquiring locks in a different order</li>
 * <li>Transactions that do an unbound waiting for a lock to come available</li>
 * </ol>
 * The problem with applying locks in the same order is that it places an extra borden on the developer. That is why atm the second
 * ingredient is always missing if the GammaStm (the default STM implementation) is used. Therefor a developer doesn't need to worry about
 * deadlocks (although it shifts the problem to an increased chance of starvation and livelocks).
 *
 * @author Peter Veentjer.
 * @see TxnFactoryBuilder#setReadLockMode(LockMode)
 * @see TxnFactoryBuilder#setWriteLockMode(LockMode)
 */
public interface TxnLock {

    /**
     * Returns the current LockMode. This call doesn't look at any running transaction, it shows the actual
     * state of the TxnLock. The value could be stale as soon as it is received. To retrieve the LockMode a
     * a Txn has on a TxnLock, the {@link #getLockMode()} or {@link #getLockMode(Txn)} need
     * to be used.
     *
     * @return the current LockMode.
     */
    LockMode atomicGetLockMode();

    /**
     * Gets the LockMode the transaction stored in the the {@link TxnThreadLocal} has on this TxnLock.
     * To retrieve the actual LockMode of the TxnLock, you need to use the {@link #atomicGetLockMode()}.
     *
     * @return the LockMode.
     * @throws org.multiverse.api.exceptions.TxnExecutionException
     *          if something failed while using the transaction. The transaction is guaranteed to have been aborted.
     * @throws org.multiverse.api.exceptions.ControlFlowError
     *          if the Stm needs to control the flow in a different way than normal returns of exceptions. The transaction
     *          is guaranteed to have been aborted.
     * @see #atomicGetLockMode()
     * @see #getLockMode(Txn)
     */
    LockMode getLockMode();

    /**
     * Gets the LockMode the transaction has on the TxnLock. This call makes use of the tx. To retrieve the actual
     * LockMode of the TxnLock, you need to use the {@link #atomicGetLockMode()}
     *
     * @param txn the TxnLock
     * @return the LockMode the transaction has on the TxnLock.
     * @throws org.multiverse.api.exceptions.TxnExecutionException
     *          if something failed while using the transaction. The transaction is guaranteed to have been aborted.
     * @throws org.multiverse.api.exceptions.ControlFlowError
     *          if the Stm needs to control the flow in a different way than normal returns of exceptions. The transaction
     *          is guaranteed to have been aborted.
     * @see #atomicGetLockMode()
     * @see #getLockMode(Txn)
     */
    LockMode getLockMode(Txn txn);

    /**
     * Acquires a TxnLock with the provided LockMode. This call doesn't block if the TxnLock can't be upgraded, but throws
     * a {@link org.multiverse.api.exceptions.ReadWriteConflict}. It could also  be that the TxnLock is acquired, but the
     * Txn sees that it isn't consistent anymore. In that case also a
     * {@link org.multiverse.api.exceptions.ReadWriteConflict} is thrown.
     *
     * <p>This call makes use of the Txn stored in the {@link TxnThreadLocal}.
     *
     * <p>If the lockMode is lower than the LockMode the transaction already has on this TxnLock, the call is ignored.
     *
     * @param desiredLockMode the desired lockMode.
     * @throws org.multiverse.api.exceptions.TxnExecutionException
     *                              if something failed while using the transaction. The transaction is guaranteed to have been aborted.
     * @throws org.multiverse.api.exceptions.ControlFlowError
     *                              if the Stm needs to control the flow in a different way than normal returns of exceptions. The transaction
     *                              is guaranteed to have been aborted.
     * @throws NullPointerException if desiredLockMode is null. If an alive transaction is available, it will
     *                              be aborted.
     */
    void acquire(LockMode desiredLockMode);

    /**
     * Acquires a TxnLock with the provided LockMode using the provided transaction. This call doesn't block if the TxnLock can't be
     * upgraded but throws a {@link org.multiverse.api.exceptions.ReadWriteConflict}. It could also be that the TxnLock is acquired,
     * but the Txn sees that it isn't consistent anymore. In that case also a
     * {@link org.multiverse.api.exceptions.ReadWriteConflict} is thrown.
     *
     * <p>If the lockMode is lower than the LockMode the transaction already has on this TxnLock, the call is ignored.
     *
     * @param txn              the Txn used for this operation.
     * @param desiredLockMode the desired lockMode.
     * @throws org.multiverse.api.exceptions.TxnExecutionException
     *                              if something failed while using the transaction. The transaction is guaranteed to have been aborted.
     * @throws org.multiverse.api.exceptions.ControlFlowError
     *                              if the Stm needs to control the flow in a different way than normal returns of exceptions. The transaction
     *                              is guaranteed to have been aborted.
     * @throws NullPointerException if tx or desiredLockMode is null. If an alive transaction is available, it will
     *                              be aborted.
     */
    void acquire(Txn txn, LockMode desiredLockMode);
}
