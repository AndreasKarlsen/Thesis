/**
 * Created by Kasper on 03-02-2015.
 */
import org.multiverse.api.references.*;

import java.util.Date;

import static org.multiverse.api.StmUtils.*;

public class Account{

    private final TxnRef<Date> lastUpdate;
    private final TxnInteger balance;

    public Account(int balance){
        this.lastUpdate = newTxnRef(new Date());
        this.balance = newTxnInteger(balance);
    }

    public void incBalance(final int amount, final Date date){
        atomic(() -> {
            balance.increment(amount);
            lastUpdate.set(date);

            if(balance.get()<0){
                throw new IllegalStateException("Not enough money");
            }
        });
    }

    @Override public String toString() {
        final String[] result = {null};
        atomic(() -> {
            result[0] = "Account{" +
                    "lastUpdate=" + lastUpdate +
                    ", balance=" + balance +
                    '}';
        });
        return result[0];
    }
}