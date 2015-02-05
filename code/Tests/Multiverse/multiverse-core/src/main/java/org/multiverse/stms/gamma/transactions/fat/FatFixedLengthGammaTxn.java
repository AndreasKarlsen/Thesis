package org.multiverse.stms.gamma.transactions.fat;

import org.multiverse.api.lifecycle.TxnEvent;
import org.multiverse.stms.gamma.GammaStm;
import org.multiverse.stms.gamma.Listeners;
import org.multiverse.stms.gamma.transactionalobjects.BaseGammaTxnRef;
import org.multiverse.stms.gamma.transactionalobjects.GammaObject;
import org.multiverse.stms.gamma.transactionalobjects.Tranlocal;
import org.multiverse.stms.gamma.transactions.GammaTxn;
import org.multiverse.stms.gamma.transactions.GammaTxnConfig;

import static org.multiverse.utils.Bugshaker.shakeBugs;

/**
 * A Fat {@link org.multiverse.stms.gamma.transactions.GammaTxn} (supporting all features) but has a fixed capacity.
 *
 * @author Peter Veentjer.
 */
public final class FatFixedLengthGammaTxn extends GammaTxn {

    public Tranlocal head;
    public int size = 0;
    public boolean hasReads = false;
    public long localConflictCount;
    public final Listeners[] listenersArray;

    public FatFixedLengthGammaTxn(final GammaStm stm) {
        this(new GammaTxnConfig(stm));
    }

    @SuppressWarnings({"ObjectAllocationInLoop"})
    public FatFixedLengthGammaTxn(final GammaTxnConfig config) {
        super(config, TRANSACTIONTYPE_FAT_FIXED_LENGTH);

        listenersArray = new Listeners[config.maxFixedLengthTransactionSize];

        Tranlocal h = null;
        for (int k = 0; k < config.maxFixedLengthTransactionSize; k++) {
            Tranlocal newNode = new Tranlocal();
            if (h != null) {
                h.previous = newNode;
                newNode.next = h;
            }

            h = newNode;
        }
        head = h;
    }

    @Override
    public final void commit() {
        if (status == TX_COMMITTED) {
            return;
        }

        if (status != TX_ACTIVE && status != TX_PREPARED) {
            throw abortCommitOnBadStatus();
        }

        if (abortOnly) {
            throw abortCommitOnAbortOnly();
        }

        if (status == TX_ACTIVE) {
            notifyListeners(TxnEvent.PrePrepare);
        }

        if (size > 0) {
            if (hasWrites) {
                if (status == TX_ACTIVE) {
                    GammaObject o = prepareChainForCommit();
                    if (o != null) {
                        throw abortOnReadWriteConflict(o);
                    }
                }

                if (commitConflict) {
                    config.globalConflictCounter.signalConflict();
                }

                final Listeners[] listenersArray = commitChain();
                if (listenersArray != null) {
                    Listeners.openAll(listenersArray, pool);
                }
            } else {
                releaseChain(true);
            }
        }

        status = TX_COMMITTED;
        notifyListeners(TxnEvent.PostCommit);
    }

    private Listeners[] commitChain() {
        int listenersIndex = 0;
        Tranlocal node = head;
        do {
            if (SHAKE_BUGS) shakeBugs();

            final BaseGammaTxnRef owner = node.owner;
            //if we are at the end, we can return the listenersArray.
            if (owner == null) {
                return listenersArray;
            }

            final Listeners listeners = owner.commit(node, pool);
            if (listeners != null) {
                listenersArray[listenersIndex] = listeners;
                listenersIndex++;
            }
            node = node.next;
        } while (node != null);

        return listenersArray;
    }

    @Override
    public final void prepare() {
        if (status == TX_PREPARED) {
            return;
        }

        if (status != TX_ACTIVE) {
            throw abortPrepareOnBadStatus();
        }

        if (abortOnly) {
            throw abortPrepareOnAbortOnly();
        }

        notifyListeners(TxnEvent.PrePrepare);

        GammaObject o = prepareChainForCommit();
        if (o != null) {
            throw abortOnReadWriteConflict(o);
        }

        status = TX_PREPARED;
    }

    @SuppressWarnings({"BooleanMethodIsAlwaysInverted"})
    private BaseGammaTxnRef prepareChainForCommit() {
        if (skipPrepare()) {
            return null;
        }

        Tranlocal node = head;

        do {
            final BaseGammaTxnRef owner = node.owner;

            if (owner == null) {
                return null;
            }

            if (SHAKE_BUGS) shakeBugs();
            if (!owner.prepare(this, node)) {
                return owner;
            }

            node = node.next;
        } while (node != null);

        return null;
    }

    @Override
    public final void abort() {
        if (status == TX_ABORTED) {
            return;
        }

        if (status == TX_COMMITTED) {
            throw failAbortOnAlreadyCommitted();
        }

        releaseChain(false);
        status = TX_ABORTED;
        notifyListeners(TxnEvent.PostAbort);
    }

    private void releaseChain(final boolean success) {
        Tranlocal node = head;
        while (node != null) {
            final BaseGammaTxnRef owner = node.owner;

            if (owner == null) {
                return;
            }

            if (SHAKE_BUGS) shakeBugs();
            if (success) {
                owner.releaseAfterReading(node, pool);
            } else {
                owner.releaseAfterFailure(node, pool);
            }

            node = node.next;
        }
    }

    @Override
    public final Tranlocal getRefTranlocal(final BaseGammaTxnRef ref) {
        Tranlocal node = head;
        while (node != null) {
            //noinspection ObjectEquality
            if (node.owner == ref) {
                return node;
            }

            if (node.owner == null) {
                return null;
            }

            node = node.next;
        }
        return null;
    }

    @Override
    public final void retry() {
        if (status != TX_ACTIVE) {
            throw abortRetryOnBadStatus();
        }

        if (!config.isBlockingAllowed()) {
            throw abortRetryOnNoBlockingAllowed();
        }

        if (size == 0) {
            throw abortRetryOnNoRetryPossible();
        }

        retryListener.reset();
        final long listenerEra = retryListener.getEra();

        boolean furtherRegistrationNeeded = true;
        boolean atLeastOneRegistration = false;

        Tranlocal tranlocal = head;
        do {
            final BaseGammaTxnRef owner = tranlocal.owner;

            if (furtherRegistrationNeeded) {
                switch (owner.registerChangeListener(retryListener, tranlocal, pool, listenerEra)) {
                    case REGISTRATION_DONE:
                        atLeastOneRegistration = true;
                        break;
                    case REGISTRATION_NOT_NEEDED:
                        furtherRegistrationNeeded = false;
                        atLeastOneRegistration = true;
                        break;
                    case REGISTRATION_NONE:
                        break;
                    default:
                        throw new IllegalStateException();
                }
            }

            owner.releaseAfterFailure(tranlocal, pool);
            tranlocal = tranlocal.next;
        } while (tranlocal != null && tranlocal.owner != null);

        status = TX_ABORTED;

        if (!atLeastOneRegistration) {
            throw abortRetryOnNoRetryPossible();
        }

        throw newRetryError();
    }


    @Override
    public final Tranlocal locate(BaseGammaTxnRef o) {
        if (status != TX_ACTIVE) {
            throw abortLocateOnBadStatus(o);
        }

        if (o == null) {
            throw abortLocateOnNullArgument();
        }

        return getRefTranlocal(o);
    }

    @Override
    public final void hardReset() {
        if (listeners != null) {
            listeners.clear();
            pool.putArrayList(listeners);
            listeners = null;
        }

        status = TX_ACTIVE;
        hasWrites = false;
        size = 0;
        remainingTimeoutNs = config.timeoutNs;
        richmansMansConflictScan = config.speculativeConfiguration.get().richMansConflictScanRequired;
        attempt = 1;
        hasReads = false;
        abortOnly = false;
        commitConflict = false;
        evaluatingCommute = false;
    }

    @Override
    public final boolean softReset() {
        if (attempt >= config.getMaxRetries()) {
            return false;
        }

        if (listeners != null) {
            listeners.clear();
            pool.putArrayList(listeners);
            listeners = null;
        }

        commitConflict = false;
        status = TX_ACTIVE;
        hasWrites = false;
        size = 0;
        hasReads = false;
        abortOnly = false;
        attempt++;
        evaluatingCommute = false;
        return true;
    }

    public final void shiftInFront(Tranlocal newHead) {
        //noinspection ObjectEquality
        if (newHead == head) {
            return;
        }


        head.previous = newHead;
        if (newHead.next != null) {
            newHead.next.previous = newHead.previous;
        }
        newHead.previous.next = newHead.next;
        newHead.next = head;
        newHead.previous = null;
        head = newHead;
    }

    @Override
    public final boolean isReadConsistent(Tranlocal justAdded) {
        if (!hasReads) {
            return true;
        }

        if (config.readLockModeAsInt > LOCKMODE_NONE) {
            return true;
        }

        if (config.inconsistentReadAllowed) {
            return true;
        }

        if (richmansMansConflictScan) {
            if (SHAKE_BUGS) shakeBugs();

            final long currentConflictCount = config.globalConflictCounter.count();

            if (localConflictCount == currentConflictCount) {
                return true;
            }

            localConflictCount = currentConflictCount;
            //we are going to fall through to do a full conflict scan
        } else if (size > config.maximumPoorMansConflictScanLength) {
            throw abortOnRichmanConflictScanDetected();
        }

        //doing a full conflict scan
        Tranlocal node = head;
        while (node != null) {
            if (SHAKE_BUGS) shakeBugs();

            //if we are at the end, we are done.
            if (node.owner == null) {
                break;
            }

            final boolean skip = !richmansMansConflictScan && node == justAdded;
            if (!skip && node.owner.hasReadConflict(node)) {
                return false;
            }

            node = node.next;
        }

        return true;
    }

    @Override
    public void initLocalConflictCounter() {
        if (richmansMansConflictScan && !hasReads) {
            localConflictCount = config.globalConflictCounter.count();
        }
    }
}
